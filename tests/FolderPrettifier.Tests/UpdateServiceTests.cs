using FolderPrettifier;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class UpdateServiceTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Responder { get; set; }

            public List<HttpRequestMessage> Requests { get; private set; }

            public FakeHttpMessageHandler()
            {
                Requests = new List<HttpRequestMessage>();
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Responder(request, cancellationToken);
            }
        }

        private static string ReleaseJson(string tag, string notes = "notes", params string[] assetNames)
        {
            StringBuilder assets = new StringBuilder();
            foreach (string name in assetNames)
            {
                assets.Append("{\"name\": \"" + name + "\", \"browser_download_url\": \"https://github.com/test/repo/releases/download/" + tag + "/" + name + "\", \"size\": 12345},");
            }
            if (assets.Length > 0)
            {
                assets.Length -= 1;
            }
            return "{\"tag_name\": \"" + tag + "\", \"html_url\": \"https://github.com/test/repo/releases/tag/" + tag + "\", \"body\": \"" + notes + "\", \"assets\": [" + assets + "]}";
        }

        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "fpf-update-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        private static UpdateService CreateService(Func<bool> is64Bit = null)
        {
            return new UpdateService(
                handler: new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)) },
                is64BitProcess: is64Bit,
                releasesApiUrl: "https://api.test/releases/latest",
                releasesPageUrl: "https://github.com/test/repo/releases");
        }

        [Test]
        public void ParseReleaseJson_NewerVersion_ReturnsUpdateInfo()
        {
            UpdateService service = CreateService();

            UpdateInfo info = service.ParseReleaseJson(ReleaseJson("v2.2.0", "Fixed stuff"), new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Version, Is.EqualTo(new Version(2, 2, 0, 0)));
            Assert.That(info.VersionTag, Is.EqualTo("v2.2.0"));
            Assert.That(info.ReleaseNotes, Is.EqualTo("Fixed stuff"));
            Assert.That(info.ReleasePageUrl, Is.EqualTo("https://github.com/test/repo/releases/tag/v2.2.0"));
        }

        [Test]
        public void ParseReleaseJson_SameVersion_ReturnsNull()
        {
            UpdateService service = CreateService();

            UpdateInfo info = service.ParseReleaseJson(ReleaseJson("v2.1.0"), new Version("2.1.0.0"));

            Assert.That(info, Is.Null);
        }

        [Test]
        public void ParseReleaseJson_OlderVersion_ReturnsNull()
        {
            UpdateService service = CreateService();

            UpdateInfo info = service.ParseReleaseJson(ReleaseJson("v2.0.0"), new Version("2.1.0.0"));

            Assert.That(info, Is.Null);
        }

        [Test]
        public void ParseReleaseJson_ThreePartTag_IsNormalizedForComparison()
        {
            UpdateService service = CreateService();

            UpdateInfo info = service.ParseReleaseJson(ReleaseJson("v2.1.0"), new Version("2.1.0.0"));

            Assert.That(info, Is.Null, "Three-part tag equal to four-part current version should not be an update");
        }

        [Test]
        public void ParseReleaseJson_PicksX64Asset_When64Bit()
        {
            UpdateService service = CreateService(is64Bit: () => true);
            string json = ReleaseJson("v2.2.0",
                "FolderPrettifier-Setup-2.2.0.exe",
                "FolderPrettifier-Portable-x86-2.2.0.exe",
                "FolderPrettifier-Portable-x64-2.2.0.exe");

            UpdateInfo info = service.ParseReleaseJson(json, new Version("2.1.0.0"));

            Assert.That(info.AssetName, Is.EqualTo("FolderPrettifier-Portable-x64-2.2.0.exe"));
            Assert.That(info.AssetUrl, Is.EqualTo("https://github.com/test/repo/releases/download/v2.2.0/FolderPrettifier-Portable-x64-2.2.0.exe"));
        }

        [Test]
        public void ParseReleaseJson_PicksX86Asset_When32Bit()
        {
            UpdateService service = CreateService(is64Bit: () => false);
            string json = ReleaseJson("v2.2.0",
                "FolderPrettifier-Portable-x64-2.2.0.exe",
                "FolderPrettifier-Portable-x86-2.2.0.exe");

            UpdateInfo info = service.ParseReleaseJson(json, new Version("2.1.0.0"));

            Assert.That(info.AssetName, Is.EqualTo("FolderPrettifier-Portable-x86-2.2.0.exe"));
        }

        [Test]
        public void ParseReleaseJson_NoMatchingAsset_UsesReleasePageFallback()
        {
            UpdateService service = CreateService();
            string json = ReleaseJson("v2.2.0", "notes", "FolderPrettifier-Setup-2.2.0.exe");

            UpdateInfo info = service.ParseReleaseJson(json, new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.AssetName, Is.Null);
            Assert.That(info.AssetUrl, Is.Null);
            Assert.That(info.ReleasePageUrl, Is.EqualTo("https://github.com/test/repo/releases/tag/v2.2.0"));
        }

        [Test]
        public void ParseReleaseJson_PicksLegacyAsset_WhenNoPortableAssets()
        {
            UpdateService service = CreateService(is64Bit: () => true);
            string json = ReleaseJson("v2.0.0", "notes",
                "Folder.Prettifier.32-bit.exe",
                "Folder.Prettifier.64-bit.exe");

            UpdateInfo info = service.ParseReleaseJson(json, new Version("1.5.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.AssetName, Is.EqualTo("Folder.Prettifier.64-bit.exe"));
            Assert.That(info.AssetUrl, Is.EqualTo("https://github.com/test/repo/releases/download/v2.0.0/Folder.Prettifier.64-bit.exe"));
        }

        [Test]
        public void ParseReleaseJson_PicksLegacyAsset_When32Bit()
        {
            UpdateService service = CreateService(is64Bit: () => false);
            string json = ReleaseJson("v2.0.0", "notes",
                "Folder.Prettifier.64-bit.exe",
                "Folder.Prettifier.32-bit.exe");

            UpdateInfo info = service.ParseReleaseJson(json, new Version("1.5.0.0"));

            Assert.That(info.AssetName, Is.EqualTo("Folder.Prettifier.32-bit.exe"));
        }

        [Test]
        public void ParseReleaseJson_NoHtmlUrl_UsesConfiguredReleasePage()
        {
            UpdateService service = CreateService();
            string json = "{\"tag_name\": \"v2.2.0\", \"assets\": []}";

            UpdateInfo info = service.ParseReleaseJson(json, new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.ReleasePageUrl, Is.EqualTo("https://github.com/test/repo/releases"));
        }

        [Test]
        public void ParseReleaseJson_InvalidJson_ReturnsNull()
        {
            UpdateService service = CreateService();

            Assert.That(service.ParseReleaseJson("not json", new Version("2.1.0.0")), Is.Null);
        }

        [Test]
        public void ParseReleaseJson_EmptyJson_ReturnsNull()
        {
            UpdateService service = CreateService();

            Assert.That(service.ParseReleaseJson("", new Version("2.1.0.0")), Is.Null);
        }

        [Test]
        public void ParseReleaseJson_MissingTag_ReturnsNull()
        {
            UpdateService service = CreateService();

            Assert.That(service.ParseReleaseJson("{\"assets\": []}", new Version("2.1.0.0")), Is.Null);
        }

        [Test]
        public void ParseReleaseJson_UnparseableTag_ReturnsNull()
        {
            UpdateService service = CreateService();

            Assert.That(service.ParseReleaseJson(ReleaseJson("v2.1.0-rc1"), new Version("2.1.0.0")), Is.Null);
        }

        [Test]
        public void ParseReleaseJson_NullCurrentVersion_ReturnsNull()
        {
            UpdateService service = CreateService();

            Assert.That(service.ParseReleaseJson(ReleaseJson("v2.2.0"), null), Is.Null);
        }

        [Test]
        public async Task CheckForUpdateAsync_HttpError_ReturnsNull()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)) };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");

            UpdateInfo info = await service.CheckForUpdateAsync(new Version("2.1.0.0"));

            Assert.That(info, Is.Null);
        }

        [Test]
        public async Task CheckForUpdateAsync_NetworkFailure_ReturnsNull()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => { throw new IOException("network down"); } };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");

            UpdateInfo info = await service.CheckForUpdateAsync(new Version("2.1.0.0"));

            Assert.That(info, Is.Null);
        }

        [Test]
        public async Task CheckForUpdateAsync_NewerRelease_ReturnsUpdateInfo()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ReleaseJson("v2.2.0", "notes", "FolderPrettifier-Portable-x86-2.2.0.exe"))
                })
            };
            UpdateService service = new UpdateService(handler: handler, is64BitProcess: () => false, releasesApiUrl: "https://api.test/releases/latest");

            UpdateInfo info = await service.CheckForUpdateAsync(new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Version, Is.EqualTo(new Version(2, 2, 0, 0)));
            Assert.That(info.AssetName, Is.EqualTo("FolderPrettifier-Portable-x86-2.2.0.exe"));
        }

        [Test]
        public async Task CheckForUpdateAsync_SendsUserAgent()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ReleaseJson("v2.2.0"))
                })
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");

            await service.CheckForUpdateAsync(new Version("2.1.0.0"));

            Assert.That(handler.Requests, Has.Count.EqualTo(1));
            Assert.That(handler.Requests[0].Headers.UserAgent.ToString(), Does.Contain("FolderPrettifier"));
        }

        private class SyncProgress : IProgress<int>
        {
            public List<int> Values { get; private set; }

            public SyncProgress()
            {
                Values = new List<int>();
            }

            public void Report(int value)
            {
                Values.Add(value);
            }
        }

        [Test]
        public async Task DownloadAsync_WritesFileAndReportsProgress()
        {
            byte[] payload = new byte[200000];
            new Random(42).NextBytes(payload);
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(payload)
                })
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");
            UpdateInfo update = new UpdateInfo
            {
                AssetName = "FolderPrettifier-Portable-x86-2.2.0.exe",
                AssetUrl = "https://api.test/download/update.exe"
            };
            string dest = Path.Combine(_tempDir, "update.exe");
            SyncProgress progress = new SyncProgress();

            bool result = await service.DownloadAsync(update, dest, progress);

            Assert.That(result, Is.True);
            Assert.That(File.ReadAllBytes(dest), Is.EqualTo(payload));
            Assert.That(progress.Values, Has.Count.GreaterThan(1));
            Assert.That(progress.Values[progress.Values.Count - 1], Is.EqualTo(100));
        }

        [Test]
        public async Task DownloadAsync_HttpError_ReturnsFalse()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound))
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");
            UpdateInfo update = new UpdateInfo { AssetUrl = "https://api.test/download/update.exe" };

            bool result = await service.DownloadAsync(update, Path.Combine(_tempDir, "update.exe"), null);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DownloadAsync_ExistingFile_IsSkipped()
        {
            string dest = Path.Combine(_tempDir, "update.exe");
            File.WriteAllText(dest, "already downloaded");
            bool requested = false;
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => { requested = true; return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)); }
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");
            UpdateInfo update = new UpdateInfo { AssetUrl = "https://api.test/download/update.exe" };

            bool result = await service.DownloadAsync(update, dest, null);

            Assert.That(result, Is.True);
            Assert.That(requested, Is.False);
            Assert.That(File.ReadAllText(dest), Is.EqualTo("already downloaded"));
        }

        [Test]
        public async Task DownloadAsync_NoAssetUrl_ReturnsFalse()
        {
            UpdateService service = CreateService();
            UpdateInfo update = new UpdateInfo { AssetUrl = null };

            bool result = await service.DownloadAsync(update, Path.Combine(_tempDir, "update.exe"), null);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ApplyUpdate_MissingFile_ReturnsFalse()
        {
            UpdateService service = CreateService();

            Assert.That(service.ApplyUpdate(Path.Combine(_tempDir, "missing.exe")), Is.False);
        }
    }
}
