using FolderPrettifier;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public void LaunchUpdater_MissingFile_ReturnsFalse()
        {
            UpdateService service = CreateService();

            Assert.That(service.LaunchUpdater(Path.Combine(_tempDir, "missing.exe")), Is.False);
        }

        [Test]
        public void LaunchUpdater_EmptyPath_ReturnsFalse()
        {
            UpdateService service = CreateService();

            Assert.That(service.LaunchUpdater(""), Is.False);
            Assert.That(service.LaunchUpdater("   "), Is.False);
            Assert.That(service.LaunchUpdater(null), Is.False);
        }

        [Test]
        public void BuildUpdaterArguments_QuotesTargetPathWithSpaces()
        {
            string args = UpdateService.BuildUpdaterArguments(@"C:\Program Files\Folder Prettifier\Folder Prettifier.exe", 1234);

            Assert.That(args, Is.EqualTo("--apply-update \"C:\\Program Files\\Folder Prettifier\\Folder Prettifier.exe\" 1234"));
        }

        [Test]
        public void BuildUpdaterArguments_IncludesPid()
        {
            string args = UpdateService.BuildUpdaterArguments(@"C:\app.exe", 98765);

            Assert.That(args, Is.EqualTo("--apply-update \"C:\\app.exe\" 98765"));
        }

        [Test]
        public void TryParseUpdaterArgs_ValidArgs_ReturnsTrue()
        {
            string targetExe;
            int oldPid;

            bool result = UpdateService.TryParseUpdaterArgs(
                new[] { "--apply-update", @"C:\Program Files\Folder Prettifier\Folder Prettifier.exe", "1234" },
                out targetExe, out oldPid);

            Assert.That(result, Is.True);
            Assert.That(targetExe, Is.EqualTo(@"C:\Program Files\Folder Prettifier\Folder Prettifier.exe"));
            Assert.That(oldPid, Is.EqualTo(1234));
        }

        [Test]
        public void TryParseUpdaterArgs_FirstArgNotApplyUpdate_ReturnsFalse()
        {
            string targetExe;
            int oldPid;

            bool result = UpdateService.TryParseUpdaterArgs(
                new[] { @"C:\some folder", "ignored", "ignored" },
                out targetExe, out oldPid);

            Assert.That(result, Is.False);
            Assert.That(targetExe, Is.Null);
            Assert.That(oldPid, Is.EqualTo(0));
        }

        [Test]
        public void TryParseUpdaterArgs_TooFewArgs_ReturnsFalse()
        {
            string targetExe;
            int oldPid;

            Assert.That(UpdateService.TryParseUpdaterArgs(new[] { "--apply-update", @"C:\app.exe" }, out targetExe, out oldPid), Is.False);
            Assert.That(UpdateService.TryParseUpdaterArgs(new[] { "--apply-update" }, out targetExe, out oldPid), Is.False);
            Assert.That(UpdateService.TryParseUpdaterArgs(new string[0], out targetExe, out oldPid), Is.False);
            Assert.That(UpdateService.TryParseUpdaterArgs(null, out targetExe, out oldPid), Is.False);
        }

        [Test]
        public void TryParseUpdaterArgs_NonNumericPid_ReturnsFalse()
        {
            string targetExe;
            int oldPid;

            bool result = UpdateService.TryParseUpdaterArgs(
                new[] { "--apply-update", @"C:\app.exe", "abc" },
                out targetExe, out oldPid);

            Assert.That(result, Is.False);
        }

        [Test]
        public void TryParseUpdaterArgs_EmptyTargetPath_ReturnsFalse()
        {
            string targetExe;
            int oldPid;

            Assert.That(UpdateService.TryParseUpdaterArgs(new[] { "--apply-update", "", "123" }, out targetExe, out oldPid), Is.False);
            Assert.That(UpdateService.TryParseUpdaterArgs(new[] { "--apply-update", "   ", "123" }, out targetExe, out oldPid), Is.False);
        }

        [Test]
        public void RoundTrip_BuildThenParse_ReturnsOriginalValues()
        {
            string target = @"C:\Program Files\Folder Prettifier\Folder Prettifier.exe";
            int pid = 424242;

            string argsLine = UpdateService.BuildUpdaterArguments(target, pid);
            string[] args = CommandLineToArgs(argsLine);

            string parsedTarget;
            int parsedPid;
            Assert.That(UpdateService.TryParseUpdaterArgs(args, out parsedTarget, out parsedPid), Is.True);
            Assert.That(parsedTarget, Is.EqualTo(target));
            Assert.That(parsedPid, Is.EqualTo(pid));
        }

        private static string[] CommandLineToArgs(string commandLine)
        {
            List<string> result = new List<string>();
            StringBuilder current = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in commandLine)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ' ' && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                }
                else
                {
                    current.Append(c);
                }
            }

            if (current.Length > 0)
            {
                result.Add(current.ToString());
            }

            return result.ToArray();
        }

        [Test]
        public void RunUpdater_WaitsForOldProcessExit_ThenCopiesAndRelaunches()
        {
            string markerPath = Path.Combine(_tempDir, "marker.txt");
            string updaterExe = Path.Combine(_tempDir, "new version.cmd");
            string targetExe = Path.Combine(_tempDir, "old app.cmd");
            File.WriteAllText(updaterExe, "@echo off\r\necho UPDATED> \"" + markerPath + "\"\r\n");
            File.WriteAllText(targetExe, "@echo off\r\necho OLD\r\n");

            Process oldProcess = Process.Start(new ProcessStartInfo("cmd.exe", "/c ping 127.0.0.1 -n 4 >nul")
            {
                UseShellExecute = false,
                CreateNoWindow = true
            });
            Assert.That(oldProcess, Is.Not.Null);

            UpdateService.RunUpdater(updaterExe, targetExe, oldProcess.Id, 5, 100);

            Assert.That(oldProcess.HasExited, Is.True, "Updater must wait for the old process to exit");
            Assert.That(File.ReadAllText(targetExe), Is.EqualTo(File.ReadAllText(updaterExe)), "Target must be replaced with the new version");

            DateTime deadline = DateTime.UtcNow.AddSeconds(10);
            while (!File.Exists(markerPath) && DateTime.UtcNow < deadline)
            {
                System.Threading.Thread.Sleep(100);
            }
            Assert.That(File.Exists(markerPath), Is.True, "Relaunched target must run");
        }

        [Test]
        public void RunUpdater_OldProcessAlreadyExited_StillCopiesAndRelaunches()
        {
            string markerPath = Path.Combine(_tempDir, "marker2.txt");
            string updaterExe = Path.Combine(_tempDir, "new2.cmd");
            string targetExe = Path.Combine(_tempDir, "old2.cmd");
            File.WriteAllText(updaterExe, "@echo off\r\necho UPDATED2> \"" + markerPath + "\"\r\n");
            File.WriteAllText(targetExe, "@echo off\r\necho OLD2\r\n");

            UpdateService.RunUpdater(updaterExe, targetExe, int.MaxValue, 5, 100);

            Assert.That(File.ReadAllText(targetExe), Is.EqualTo(File.ReadAllText(updaterExe)));

            DateTime deadline = DateTime.UtcNow.AddSeconds(10);
            while (!File.Exists(markerPath) && DateTime.UtcNow < deadline)
            {
                System.Threading.Thread.Sleep(100);
            }
            Assert.That(File.Exists(markerPath), Is.True);
        }

        [Test]
        public void RunUpdater_MissingUpdaterFile_TargetUnchanged()
        {
            string targetExe = Path.Combine(_tempDir, "old3.cmd");
            File.WriteAllText(targetExe, "@echo off\r\n");

            UpdateService.RunUpdater(Path.Combine(_tempDir, "missing.cmd"), targetExe, int.MaxValue, 2, 10);

            Assert.That(File.ReadAllText(targetExe), Is.EqualTo("@echo off\r\n"));
        }

        [Test]
        public void RunUpdater_MissingTargetDirectory_NoThrow()
        {
            string updaterExe = Path.Combine(_tempDir, "new4.cmd");
            File.WriteAllText(updaterExe, "@echo off\r\n");

            Assert.DoesNotThrow(() =>
                UpdateService.RunUpdater(updaterExe, Path.Combine(_tempDir, "missing", "app.cmd"), int.MaxValue, 2, 10));
        }

        [Test]
        public void RunUpdater_NullOrEmptyPaths_NoThrow()
        {
            Assert.DoesNotThrow(() => UpdateService.RunUpdater(null, null, 1, 1, 1));
            Assert.DoesNotThrow(() => UpdateService.RunUpdater("", "", 1, 1, 1));
        }

        [Test]
        public void ParseReleaseJson_UppercaseVPrefix_ReturnsUpdate()
        {
            UpdateService service = CreateService();

            UpdateInfo info = service.ParseReleaseJson(ReleaseJson("V2.2.0"), new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Version, Is.EqualTo(new Version(2, 2, 0, 0)));
        }

        [Test]
        public void ParseReleaseJson_WhitespaceAroundTag_ReturnsUpdate()
        {
            UpdateService service = CreateService();

            UpdateInfo info = service.ParseReleaseJson(ReleaseJson("   v2.2.0   "), new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Version, Is.EqualTo(new Version(2, 2, 0, 0)));
        }

        [Test]
        public void ParseReleaseJson_NoVPrefix_ReturnsUpdate()
        {
            UpdateService service = CreateService();

            UpdateInfo info = service.ParseReleaseJson(ReleaseJson("2.2.0"), new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Version, Is.EqualTo(new Version(2, 2, 0, 0)));
        }

        [Test]
        public void ParseReleaseJson_FourPartTag_ComparedCorrectly()
        {
            UpdateService service = CreateService();

            UpdateInfo info = service.ParseReleaseJson(ReleaseJson("v2.2.0.1"), new Version("2.2.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.Version, Is.EqualTo(new Version(2, 2, 0, 1)));
        }

        [Test]
        public void ParseReleaseJson_TwoPartTag_NormalizedToFourPart()
        {
            UpdateService service = CreateService();

            Assert.That(service.ParseReleaseJson(ReleaseJson("v2.2"), new Version("2.2.0.0")), Is.Null);
            Assert.That(service.ParseReleaseJson(ReleaseJson("v2.1"), new Version("2.2.0.0")), Is.Null);

            UpdateInfo info = service.ParseReleaseJson(ReleaseJson("v2.3"), new Version("2.2.0.0"));
            Assert.That(info, Is.Not.Null);
            Assert.That(info.Version, Is.EqualTo(new Version(2, 3, 0, 0)));
        }

        [Test]
        public void ParseReleaseJson_NewStyleAssetPreferredOverLegacy()
        {
            UpdateService service = CreateService(is64Bit: () => true);
            string json = ReleaseJson("v2.2.0",
                "Folder.Prettifier.64-bit.exe",
                "FolderPrettifier-Portable-x64-2.2.0.exe");

            UpdateInfo info = service.ParseReleaseJson(json, new Version("2.1.0.0"));

            Assert.That(info.AssetName, Is.EqualTo("FolderPrettifier-Portable-x64-2.2.0.exe"));
        }

        [Test]
        public void ParseReleaseJson_LegacyAssetOfOtherArchitecture_NotPicked()
        {
            UpdateService service = CreateService(is64Bit: () => false);
            string json = ReleaseJson("v2.2.0", "Folder.Prettifier.64-bit.exe");

            UpdateInfo info = service.ParseReleaseJson(json, new Version("2.1.0.0"));

            Assert.That(info.AssetName, Is.Null);
            Assert.That(info.AssetUrl, Is.Null);
            Assert.That(info.ReleasePageUrl, Is.EqualTo("https://github.com/test/repo/releases/tag/v2.2.0"));
        }

        [Test]
        public void ParseReleaseJson_MissingAssetsKey_UsesReleasePageFallback()
        {
            UpdateService service = CreateService();
            string json = "{\"tag_name\": \"v2.2.0\", \"html_url\": \"https://github.com/test/repo/releases/tag/v2.2.0\"}";

            UpdateInfo info = service.ParseReleaseJson(json, new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.AssetName, Is.Null);
            Assert.That(info.AssetUrl, Is.Null);
            Assert.That(info.ReleasePageUrl, Is.EqualTo("https://github.com/test/repo/releases/tag/v2.2.0"));
        }

        [Test]
        public void ParseReleaseJson_MissingBody_EmptyReleaseNotes()
        {
            UpdateService service = CreateService();
            string json = "{\"tag_name\": \"v2.2.0\", \"assets\": []}";

            UpdateInfo info = service.ParseReleaseJson(json, new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.ReleaseNotes, Is.EqualTo(""));
        }

        [Test]
        public void ParseReleaseJson_EmptyHtmlUrl_UsesConfiguredReleasePage()
        {
            UpdateService service = CreateService();
            string json = "{\"tag_name\": \"v2.2.0\", \"html_url\": \"\", \"assets\": []}";

            UpdateInfo info = service.ParseReleaseJson(json, new Version("2.1.0.0"));

            Assert.That(info, Is.Not.Null);
            Assert.That(info.ReleasePageUrl, Is.EqualTo("https://github.com/test/repo/releases"));
        }

        [Test]
        public async Task CheckForUpdateAsync_EmptyBody_ReturnsNull()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("")
                })
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");

            UpdateInfo info = await service.CheckForUpdateAsync(new Version("2.1.0.0"));

            Assert.That(info, Is.Null);
        }

        [Test]
        public async Task CheckForUpdateAsync_UsesConfiguredApiUrl()
        {
            string requestedUrl = null;
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) =>
                {
                    requestedUrl = r.RequestUri.AbsoluteUri;
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(ReleaseJson("v2.2.0"))
                    });
                }
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/custom/releases/latest");

            await service.CheckForUpdateAsync(new Version("2.1.0.0"));

            Assert.That(requestedUrl, Is.EqualTo("https://api.test/custom/releases/latest"));
        }

        private class EmptyStream : Stream
        {
            public override bool CanRead { get { return true; } }
            public override bool CanSeek { get { return false; } }
            public override bool CanWrite { get { return false; } }
            public override long Length { get { return 0; } }
            public override long Position { get { return 0; } set { } }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) { return 0; }
            public override void SetLength(long value) { }
            public override void Write(byte[] buffer, int offset, int count) { }
        }

        private class ThrowingStream : Stream
        {
            private readonly long _limit;
            private long _position;

            public ThrowingStream(long limit)
            {
                _limit = limit;
            }

            public override bool CanRead { get { return true; } }
            public override bool CanSeek { get { return false; } }
            public override bool CanWrite { get { return false; } }
            public override long Length { get { return _limit; } }
            public override long Position { get { return _position; } set { } }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_position >= _limit)
                {
                    throw new IOException("connection reset by peer");
                }
                int n = (int)Math.Min(count, Math.Min(8192, _limit - _position));
                for (int i = 0; i < n; i++)
                {
                    buffer[offset + i] = 0;
                }
                _position += n;
                return n;
            }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) { return 0; }
            public override void SetLength(long value) { }
            public override void Write(byte[] buffer, int offset, int count) { }
        }

        private class ShortStream : Stream
        {
            public override bool CanRead { get { return true; } }
            public override bool CanSeek { get { return true; } }
            public override bool CanWrite { get { return false; } }
            public override long Length { get { return 100000; } }
            public override long Position { get { return 0; } set { } }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) { return 0; }
            public override void SetLength(long value) { }
            public override void Write(byte[] buffer, int offset, int count) { }
        }

        private class UnknownLengthContent : HttpContent
        {
            private readonly byte[] _bytes;

            public UnknownLengthContent(byte[] bytes)
            {
                _bytes = bytes;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return stream.WriteAsync(_bytes, 0, _bytes.Length);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = -1;
                return false;
            }
        }

        [Test]
        public async Task DownloadAsync_EmptyContent_SucceedsWithZeroByteFile()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new EmptyStream())
                })
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");
            UpdateInfo update = new UpdateInfo { AssetUrl = "https://api.test/download/update.exe" };
            string dest = Path.Combine(_tempDir, "empty.exe");

            bool result = await service.DownloadAsync(update, dest, null);

            Assert.That(result, Is.True);
            Assert.That(File.Exists(dest), Is.True);
            Assert.That(new FileInfo(dest).Length, Is.EqualTo(0));
        }

        [Test]
        public async Task DownloadAsync_InterruptedStream_DeletesPartialFileAndReturnsFalse()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new ThrowingStream(1000000))
                })
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");
            UpdateInfo update = new UpdateInfo { AssetUrl = "https://api.test/download/update.exe" };
            string dest = Path.Combine(_tempDir, "partial.exe");

            bool result = await service.DownloadAsync(update, dest, null);

            Assert.That(result, Is.False);
            Assert.That(File.Exists(dest), Is.False, "Partial file must be removed so the next attempt re-downloads");
        }

        [Test]
        public async Task DownloadAsync_SizeMismatch_DeletesPartialFileAndReturnsFalse()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new ShortStream())
                })
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");
            UpdateInfo update = new UpdateInfo { AssetUrl = "https://api.test/download/update.exe" };
            string dest = Path.Combine(_tempDir, "short.exe");

            bool result = await service.DownloadAsync(update, dest, null);

            Assert.That(result, Is.False);
            Assert.That(File.Exists(dest), Is.False);
        }

        [Test]
        public async Task DownloadAsync_MissingDestinationDirectory_ReturnsFalse()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(new EmptyStream())
                })
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");
            UpdateInfo update = new UpdateInfo { AssetUrl = "https://api.test/download/update.exe" };
            string dest = Path.Combine(_tempDir, "no", "such", "dir", "update.exe");

            bool result = await service.DownloadAsync(update, dest, null);

            Assert.That(result, Is.False);
            Assert.That(File.Exists(dest), Is.False);
        }

        [Test]
        public async Task DownloadAsync_UnknownContentLength_StillCompletes()
        {
            byte[] payload = new byte[4096];
            new Random(42).NextBytes(payload);
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new UnknownLengthContent(payload)
                })
            };
            UpdateService service = new UpdateService(handler: handler, releasesApiUrl: "https://api.test/releases/latest");
            UpdateInfo update = new UpdateInfo { AssetUrl = "https://api.test/download/update.exe" };
            string dest = Path.Combine(_tempDir, "no-length.exe");
            SyncProgress progress = new SyncProgress();

            bool result = await service.DownloadAsync(update, dest, progress);

            Assert.That(result, Is.True);
            Assert.That(File.ReadAllBytes(dest), Is.EqualTo(payload));
            Assert.That(progress.Values, Is.EqualTo(new[] { 100 }), "Only a final 100 should be reported when the length is unknown");
        }

        [Test]
        public void CanUpdateInPlaceIn_WritableDirectory_True()
        {
            string dir = Path.Combine(_tempDir, "writable");
            Directory.CreateDirectory(dir);

            Assert.That(UpdateService.CanUpdateInPlaceIn(dir), Is.True);
            Assert.That(Directory.GetFiles(dir, ".fpf-write-probe-*"), Is.Empty, "Probe file must be cleaned up");
        }

        [Test]
        public void CanUpdateInPlaceIn_MissingDirectory_False()
        {
            Assert.That(UpdateService.CanUpdateInPlaceIn(Path.Combine(_tempDir, "missing")), Is.False);
        }

        [Test]
        public void CanUpdateInPlaceIn_NullOrEmptyDirectory_False()
        {
            Assert.That(UpdateService.CanUpdateInPlaceIn(null), Is.False);
            Assert.That(UpdateService.CanUpdateInPlaceIn(""), Is.False);
        }
    }
}
