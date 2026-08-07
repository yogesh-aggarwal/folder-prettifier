using FolderPrettifier;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class CatalogLoaderTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Responder { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Responder(request, cancellationToken);
            }
        }

        private class FakeCatalogUi : ICatalogUi
        {
            public List<string> Statuses { get; } = new List<string>();
            public List<int> Progress { get; } = new List<int>();
            public List<bool> StartEnabled { get; } = new List<bool>();
            public List<bool> RefreshEnabled { get; } = new List<bool>();

            public Version UpdateRequiredVersion { get; private set; }

            public void SetStatus(string text) { Statuses.Add(text); }
            public void SetProgress(int value) { Progress.Add(value); }
            public void SetStartEnabled(bool enabled) { StartEnabled.Add(enabled); }
            public void SetCatalogRefreshEnabled(bool enabled) { RefreshEnabled.Add(enabled); }
            public void ShowUpdateRequired(Version appVersion)
            {
                UpdateRequiredVersion = appVersion;
                RefreshEnabled.Add(true);
            }
        }

        private const string RepoApiUrl = "http://test.local/repos/folder-prettifier";
        private const string RawBaseUrlTemplate = "http://test.local/raw/{0}/catalogs/";
        private const string VersionsJson = "{ \"2.0.1\": \"v0001.jsonc\" }";
        private const string CatalogJson =
            "{\"version\": 1, \"min-app-version\": \"2.0.1\", \"default\": \"Others/Unknown\", " +
            "\"categories\": [ { \"name\": \"Videos\", \"folder\": \"Videos\", \"extensions\": [\"mp4\"] } ] }";
        private static readonly Version AppVersion = new Version(2, 1, 0);

        private string _cacheDir;
        private FakeHttpMessageHandler _handler;
        private FakeCatalogUi _ui;
        private CatalogLoader _loader;
        private readonly Dictionary<string, Tuple<HttpStatusCode, string>> _responses =
            new Dictionary<string, Tuple<HttpStatusCode, string>>(StringComparer.OrdinalIgnoreCase);

        private void Respond(string urlSuffix, HttpStatusCode status, string content)
        {
            _responses[urlSuffix] = Tuple.Create(status, content);
        }

        private void WriteCache(string fileName, string content)
        {
            if (!Directory.Exists(_cacheDir))
            {
                Directory.CreateDirectory(_cacheDir);
            }
            File.WriteAllText(Path.Combine(_cacheDir, fileName), content);
        }

        private void RespondOnline()
        {
            Respond("repos/folder-prettifier", HttpStatusCode.OK, "{ \"default_branch\": \"main\" }");
            Respond("versions.jsonc", HttpStatusCode.OK, VersionsJson);
            Respond("v0001.jsonc", HttpStatusCode.OK, CatalogJson);
        }

        [SetUp]
        public void SetUp()
        {
            _cacheDir = Path.Combine(Path.GetTempPath(), "fpcl-test-" + Guid.NewGuid().ToString("N"));
            _responses.Clear();
            _handler = new FakeHttpMessageHandler
            {
                Responder = (request, ct) =>
                {
                    string url = request.RequestUri.AbsoluteUri;
                    foreach (KeyValuePair<string, Tuple<HttpStatusCode, string>> entry in _responses)
                    {
                        if (url.EndsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            return Task.FromResult(new HttpResponseMessage(entry.Value.Item1)
                            {
                                Content = new StringContent(entry.Value.Item2)
                            });
                        }
                    }
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }
            };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(_handler);
            CatalogService service = new CatalogService(fetcher, _cacheDir,
                new CatalogBaseUrlResolver(fetcher, RepoApiUrl, RawBaseUrlTemplate, Path.Combine(_cacheDir, "repo-info.json")),
                "versions.jsonc", () => null);
            _ui = new FakeCatalogUi();
            _loader = new CatalogLoader(fetcher, service, () => AppVersion, _ui,
                internetCheckUrl: "http://test.local/internet", internetCheckTimeout: TimeSpan.FromSeconds(5), readyDelayMs: 0);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_cacheDir))
            {
                Directory.Delete(_cacheDir, true);
            }
        }

        [Test]
        public async Task LoadAsync_Success_ReturnsCatalogAndEnablesUi()
        {
            Respond("internet", HttpStatusCode.OK, "");
            RespondOnline();

            CatalogLoadResult result = await _loader.LoadAsync();

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(result.Catalog.Categories.Count, Is.EqualTo(1));
            Assert.That(_ui.Statuses[0], Is.EqualTo("Fetching Catalog..."));
            Assert.That(_ui.Statuses[_ui.Statuses.Count - 1], Is.EqualTo("Ready"));
            Assert.That(_ui.Progress, Does.Contain(0));
            Assert.That(_ui.Progress, Does.Contain(30));
            Assert.That(_ui.Progress, Does.Contain(100));
            Assert.That(_ui.StartEnabled[_ui.StartEnabled.Count - 1], Is.False,
                "Loader disables Start; the caller enables it after wiring the catalog.");
            Assert.That(_ui.RefreshEnabled[_ui.RefreshEnabled.Count - 1], Is.True);
            Assert.That(_ui.UpdateRequiredVersion, Is.Null);
        }

        [Test]
        public async Task LoadAsync_Offline_SucceedsFromCacheAndEnablesUi()
        {
            Respond("internet", HttpStatusCode.NotFound, "");
            WriteCache("versions.jsonc", VersionsJson);
            WriteCache("v0001.jsonc", CatalogJson);

            CatalogLoadResult result = await _loader.LoadAsync();

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(_ui.Statuses, Does.Contain("Offline. Using cached catalog..."));
            Assert.That(_ui.Statuses[_ui.Statuses.Count - 1], Is.EqualTo("Ready"));
        }

        [Test]
        public async Task LoadAsync_UpdateRequired_ShowsUpdateRequiredAndDisablesStart()
        {
            Respond("internet", HttpStatusCode.OK, "");
            Respond("repos/folder-prettifier", HttpStatusCode.OK, "{ \"default_branch\": \"main\" }");
            Respond("versions.jsonc", HttpStatusCode.OK, "{ \"99.0.0\": \"v0001.jsonc\" }");

            CatalogLoadResult result = await _loader.LoadAsync();

            Assert.That(result.UpdateRequired, Is.True);
            Assert.That(result.Catalog, Is.Null);
            Assert.That(_ui.UpdateRequiredVersion, Is.EqualTo(AppVersion));
            Assert.That(_ui.StartEnabled[_ui.StartEnabled.Count - 1], Is.False);
            Assert.That(_ui.RefreshEnabled[_ui.RefreshEnabled.Count - 1], Is.True);
            Assert.That(_ui.Statuses, Does.Not.Contain("Ready"));
        }

        [Test]
        public async Task LoadAsync_NoCatalog_ShowsNoCatalogAndEnablesRefresh()
        {
            Respond("internet", HttpStatusCode.OK, "");

            CatalogLoadResult result = await _loader.LoadAsync();

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Null);
            Assert.That(_ui.Statuses, Does.Contain("No catalog! Can't proceed"));
            Assert.That(_ui.RefreshEnabled[_ui.RefreshEnabled.Count - 1], Is.True);
            Assert.That(_ui.StartEnabled[_ui.StartEnabled.Count - 1], Is.False);
            Assert.That(_ui.Statuses, Does.Not.Contain("Ready"));
        }
    }
}
