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
    public class CatalogServiceTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Responder { get; set; }

            public int CallCount { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CallCount++;
                return Responder(request, cancellationToken);
            }
        }

        private const string BaseUrl = "http://test.local/catalogs/";
        private const string VersionsJson = "{ \"2.0.1\": \"v0001.jsonc\" }";
        private static readonly Version App = new Version(2, 1, 0);

        // Remote catalog (selected via index): min-app-version 2.0.1
        private const string RemoteCatalogJson =
            "{\"version\": 1, \"min-app-version\": \"2.0.1\", \"default\": \"Others/Unknown\", " +
            "\"categories\": [ { \"name\": \"Videos\", \"folder\": \"Videos\", \"extensions\": [\"mp4\"] } ] }";

        // Embedded catalog (fallback): min-app-version 2.0.0, distinct marker
        private const string EmbeddedJson =
            "{\"version\": 1, \"min-app-version\": \"2.0.0\", \"default\": \"Others/Unknown\", " +
            "\"categories\": [ { \"name\": \"Embedded\", \"folder\": \"Embedded\", \"extensions\": [\"mp4\"] } ] }";

        private string _cacheDir;
        private FakeHttpMessageHandler _handler;
        private RemoteFileFetcher _fetcher;
        private CatalogService _service;
        private readonly Dictionary<string, System.Tuple<HttpStatusCode, string>> _responses =
            new Dictionary<string, System.Tuple<HttpStatusCode, string>>(System.StringComparer.OrdinalIgnoreCase);

        private void Respond(string fileName, HttpStatusCode status, string content)
        {
            _responses[fileName] = System.Tuple.Create(status, content);
        }

        private void WriteCache(string fileName, string content)
        {
            if (!Directory.Exists(_cacheDir))
            {
                Directory.CreateDirectory(_cacheDir);
            }
            File.WriteAllText(Path.Combine(_cacheDir, fileName), content);
        }

        [SetUp]
        public void SetUp()
        {
            _cacheDir = Path.Combine(Path.GetTempPath(), "fpcs-test-" + Guid.NewGuid().ToString("N"));
            _responses.Clear();
            _handler = new FakeHttpMessageHandler
            {
                Responder = (request, ct) =>
                {
                    string url = request.RequestUri.AbsoluteUri;
                    foreach (KeyValuePair<string, System.Tuple<HttpStatusCode, string>> entry in _responses)
                    {
                        if (url.EndsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            if (entry.Value.Item1 == HttpStatusCode.NotModified)
                            {
                                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));
                            }
                            return Task.FromResult(new HttpResponseMessage(entry.Value.Item1)
                            {
                                Content = new StringContent(entry.Value.Item2)
                            });
                        }
                    }
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
                }
            };
            _fetcher = new RemoteFileFetcher(_handler);
            _service = new CatalogService(_fetcher, _cacheDir, BaseUrl, "versions.jsonc", () => EmbeddedJson);
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
        public async Task LoadAsync_OnlineFresh_LoadsSelectedCatalogAndCaches()
        {
            Respond("versions.jsonc", HttpStatusCode.OK, VersionsJson);
            Respond("v0001.jsonc", HttpStatusCode.OK, RemoteCatalogJson);

            CatalogLoadOutcome outcome = await _service.LoadAsync(App, true);

            Assert.That(outcome.UpdateRequired, Is.False);
            Assert.That(outcome.Catalog, Is.Not.Null);
            Assert.That(outcome.Catalog.MinAppVersion, Is.EqualTo("2.0.1"));
            Assert.That(outcome.FileName, Is.EqualTo("v0001.jsonc"));
            Assert.That(File.Exists(Path.Combine(_cacheDir, "versions.jsonc")), Is.True);
            Assert.That(File.Exists(Path.Combine(_cacheDir, "v0001.jsonc")), Is.True);
        }

        [Test]
        public async Task LoadAsync_OnlineVersionsUnchanged_UsesCachedVersions()
        {
            WriteCache("versions.jsonc", VersionsJson);
            Respond("versions.jsonc", HttpStatusCode.NotModified, null);
            Respond("v0001.jsonc", HttpStatusCode.OK, RemoteCatalogJson);

            CatalogLoadOutcome outcome = await _service.LoadAsync(App, true);

            Assert.That(outcome.Catalog, Is.Not.Null);
            Assert.That(outcome.Catalog.MinAppVersion, Is.EqualTo("2.0.1"));
        }

        [Test]
        public async Task LoadAsync_OnlineCatalogUnavailable_FallsBackToEmbedded()
        {
            Respond("versions.jsonc", HttpStatusCode.OK, VersionsJson);
            Respond("v0001.jsonc", HttpStatusCode.NotFound, null);

            CatalogLoadOutcome outcome = await _service.LoadAsync(App, true);

            Assert.That(outcome.UpdateRequired, Is.False);
            Assert.That(outcome.Catalog, Is.Not.Null);
            Assert.That(outcome.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }

        [Test]
        public async Task LoadAsync_OnlineCatalogIncompatible_FallsBackToEmbedded()
        {
            Respond("versions.jsonc", HttpStatusCode.OK, VersionsJson);
            Respond("v0001.jsonc", HttpStatusCode.OK, RemoteCatalogJson.Replace("2.0.1", "9.9.9"));

            CatalogLoadOutcome outcome = await _service.LoadAsync(App, true);

            Assert.That(outcome.UpdateRequired, Is.False);
            Assert.That(outcome.Catalog, Is.Not.Null);
            Assert.That(outcome.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }

        [Test]
        public async Task LoadAsync_OfflineWithCache_LoadsCachedCatalogWithoutNetwork()
        {
            WriteCache("versions.jsonc", VersionsJson);
            WriteCache("v0001.jsonc", RemoteCatalogJson);

            CatalogLoadOutcome outcome = await _service.LoadAsync(App, false);

            Assert.That(_handler.CallCount, Is.EqualTo(0));
            Assert.That(outcome.Catalog, Is.Not.Null);
            Assert.That(outcome.Catalog.MinAppVersion, Is.EqualTo("2.0.1"));
        }

        [Test]
        public async Task LoadAsync_OfflineWithoutCache_FallsBackToEmbedded()
        {
            CatalogLoadOutcome outcome = await _service.LoadAsync(App, false);

            Assert.That(_handler.CallCount, Is.EqualTo(0));
            Assert.That(outcome.Catalog, Is.Not.Null);
            Assert.That(outcome.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
            Assert.That(outcome.FileName, Is.Null);
        }

        [Test]
        public async Task LoadAsync_AppOlderThanAllIndexEntries_UpdateRequired_NoEmbeddedFallback()
        {
            Respond("versions.jsonc", HttpStatusCode.OK, "{ \"2.3.4\": \"v0002.jsonc\" }");

            CatalogLoadOutcome outcome = await _service.LoadAsync(new Version(2, 1, 0), true);

            Assert.That(outcome.UpdateRequired, Is.True);
            Assert.That(outcome.Catalog, Is.Null);
        }

        [Test]
        public async Task LoadAsync_CorruptVersions_FallsBackToEmbedded()
        {
            Respond("versions.jsonc", HttpStatusCode.OK, "corrupt !!!");

            CatalogLoadOutcome outcome = await _service.LoadAsync(App, true);

            Assert.That(outcome.UpdateRequired, Is.False);
            Assert.That(outcome.Catalog, Is.Not.Null);
            Assert.That(outcome.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }

        [Test]
        public async Task LoadAsync_OfflineStatusMessages()
        {
            List<string> statuses = new List<string>();

            await _service.LoadAsync(App, false, s => statuses.Add(s));

            Assert.That(statuses, Does.Contain("Offline. Using cached catalog..."));
            Assert.That(statuses, Does.Contain("Using embedded catalog..."));
        }

        [Test]
        public async Task LoadAsync_OnlineFreshStatusMessages()
        {
            Respond("versions.jsonc", HttpStatusCode.OK, VersionsJson);
            Respond("v0001.jsonc", HttpStatusCode.OK, RemoteCatalogJson);
            List<string> statuses = new List<string>();

            await _service.LoadAsync(App, true, s => statuses.Add(s));

            Assert.That(statuses, Does.Contain("Checking online..."));
            Assert.That(statuses, Does.Contain("Loading catalog v0001.jsonc..."));
        }
    }
}
