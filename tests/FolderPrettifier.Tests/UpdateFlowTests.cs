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
    public class UpdateFlowTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Responder { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Responder(request, cancellationToken);
            }
        }

        private class FakeUpdateService : UpdateService
        {
            public FakeUpdateService(HttpMessageHandler handler)
                : base(handler, is64BitProcess: () => true)
            {
            }

            public bool LaunchResult { get; set; } = true;

            public string LastLaunchedPath { get; private set; }

            public override bool LaunchUpdater(string downloadedFilePath)
            {
                LastLaunchedPath = downloadedFilePath;
                return LaunchResult;
            }
        }

        private class FakeUi : IUpdateUi
        {
            public List<string> Statuses { get; } = new List<string>();
            public List<int> Progress { get; } = new List<int>();
            public List<bool> CheckEnabled { get; } = new List<bool>();
            public List<string> InfoMessages { get; } = new List<string>();
            public List<string> ErrorMessages { get; } = new List<string>();
            public List<string> Confirmations { get; } = new List<string>();
            public List<string> OpenedUrls { get; } = new List<string>();

            public bool ConfirmResult { get; set; }

            public int ExitCount { get; private set; }

            public void SetStatus(string text) { Statuses.Add(text); }
            public void SetProgress(int value) { Progress.Add(value); }
            public void SetCheckEnabled(bool enabled) { CheckEnabled.Add(enabled); }
            public void ShowInfo(string text, string title) { InfoMessages.Add(text); }
            public void ShowError(string text, string title) { ErrorMessages.Add(text); }
            public bool Confirm(string text, string title) { Confirmations.Add(text); return ConfirmResult; }
            public void OpenUrl(string url) { OpenedUrls.Add(url); }
            public void Exit() { ExitCount++; }
        }

        private const string ReleaseJson =
            "{ \"tag_name\": \"v3.0.0\", \"body\": \"Fix everything\", \"html_url\": \"http://test.local/releases/v3.0.0\", " +
            "\"assets\": [ { \"name\": \"FolderPrettifier-Portable-x64-3.0.0.exe\", " +
            "\"browser_download_url\": \"http://test.local/update.exe\" } ] }";

        private static readonly Version AppVersion = new Version(2, 1, 0);

        private string _tempDir;
        private string _updatesDir;
        private FakeHttpMessageHandler _handler;
        private FakeUpdateService _service;
        private FakeUi _ui;
        private UpdateFlow _flow;

        private static UpdateInfo NewUpdate(string assetUrl = "http://test.local/update.exe")
        {
            return new UpdateInfo
            {
                Version = new Version(3, 0, 0),
                VersionTag = "v3.0.0",
                ReleaseNotes = "Fix everything",
                AssetName = "FolderPrettifier-Portable-x64-3.0.0.exe",
                AssetUrl = assetUrl,
                ReleasePageUrl = "http://test.local/releases/v3.0.0"
            };
        }

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "fpf-uf-" + Guid.NewGuid().ToString("N"));
            _updatesDir = Path.Combine(_tempDir, "updates");
            Directory.CreateDirectory(_updatesDir);
            _handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ReleaseJson)
                })
            };
            _service = new FakeUpdateService(_handler);
            _ui = new FakeUi();
            _flow = new UpdateFlow(_service, _ui, () => true, _updatesDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        private void Respond(HttpStatusCode status, string content = null)
        {
            _handler.Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(content ?? string.Empty)
            });
        }

        [Test]
        public async Task CheckForUpdateAsync_InProgress_DuplicateCallSkipped()
        {
            TaskCompletionSource<HttpResponseMessage> tcs = new TaskCompletionSource<HttpResponseMessage>();
            _handler.Responder = (r, ct) => tcs.Task;

            Task first = _flow.CheckForUpdateAsync(false, AppVersion);
            Task second = _flow.CheckForUpdateAsync(false, AppVersion);

            Task completed = await Task.WhenAny(second, Task.Delay(1000));
            Assert.That(completed, Is.SameAs(second));
            Assert.That(_ui.Statuses.FindAll(s => s == "Checking for updates...").Count, Is.EqualTo(1));
            Assert.That(_ui.CheckEnabled.Count, Is.EqualTo(1));

            tcs.SetResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            await first;
        }

        [Test]
        public async Task CheckForUpdateAsync_NoUpdate_Silent_NoDialog()
        {
            Respond(HttpStatusCode.NotFound);

            await _flow.CheckForUpdateAsync(true, AppVersion);

            Assert.That(_ui.InfoMessages.Count, Is.EqualTo(0));
            Assert.That(_ui.Confirmations.Count, Is.EqualTo(0));
            Assert.That(_ui.Statuses[_ui.Statuses.Count - 1], Is.EqualTo("Ready"));
            Assert.That(_ui.CheckEnabled[_ui.CheckEnabled.Count - 1], Is.True);
        }

        [Test]
        public async Task CheckForUpdateAsync_NoUpdate_NotSilent_ShowsUpToDateDialog()
        {
            Respond(HttpStatusCode.NotFound);

            await _flow.CheckForUpdateAsync(false, AppVersion);

            Assert.That(_ui.InfoMessages.Count, Is.EqualTo(1));
            Assert.That(_ui.InfoMessages[0], Does.Contain("latest version"));
        }

        [Test]
        public async Task CheckForUpdateAsync_UpdateAvailable_Silent_OnlyStatusHint()
        {
            await _flow.CheckForUpdateAsync(true, AppVersion);

            Assert.That(_ui.Confirmations.Count, Is.EqualTo(0));
            Assert.That(_ui.InfoMessages.Count, Is.EqualTo(0));
            Assert.That(_ui.Statuses, Does.Contain("Update available: 3.0.0.0 (Check for Updates menu)"));
        }

        [Test]
        public async Task CheckForUpdateAsync_UpdateAvailable_Declined_StaysReady()
        {
            _ui.ConfirmResult = false;

            await _flow.CheckForUpdateAsync(false, AppVersion);

            Assert.That(_ui.Confirmations.Count, Is.EqualTo(1));
            Assert.That(_ui.Confirmations[0], Does.Contain("Do you want to download and install it now?"));
            Assert.That(_ui.Statuses, Does.Not.Contain("Downloading update..."));
            Assert.That(_ui.Statuses[_ui.Statuses.Count - 1], Is.EqualTo("Ready"));
            Assert.That(_ui.CheckEnabled[_ui.CheckEnabled.Count - 1], Is.True);
        }

        [Test]
        public async Task CheckForUpdateAsync_UpdateAvailable_Accepted_ConfirmShowsVersionsAndNotes()
        {
            _ui.ConfirmResult = true;

            await _flow.CheckForUpdateAsync(false, AppVersion);

            Assert.That(_ui.Confirmations[0], Does.Contain("3.0.0"));
            Assert.That(_ui.Confirmations[0], Does.Contain("2.1.0"));
            Assert.That(_ui.Confirmations[0], Does.Contain("What's new:"));
            Assert.That(_ui.Confirmations[0], Does.Contain("Fix everything"));
            Assert.That(_ui.Statuses, Does.Contain("Downloading update..."));
            Assert.That(_ui.Statuses, Does.Contain("Applying update..."));
            Assert.That(_ui.ExitCount, Is.EqualTo(1));
        }

        [Test]
        public async Task CheckForUpdateAsync_UpdateAvailable_Accepted_EndsEnabledAndReady()
        {
            _ui.ConfirmResult = true;

            await _flow.CheckForUpdateAsync(false, AppVersion);

            Assert.That(_ui.Statuses[_ui.Statuses.Count - 1], Is.EqualTo("Ready"));
            Assert.That(_ui.CheckEnabled[_ui.CheckEnabled.Count - 1], Is.True);
        }

        [Test]
        public async Task DownloadAndApplyUpdateAsync_NoAssetUrl_ShowsManualDialogAndOpensPage()
        {
            await _flow.DownloadAndApplyUpdateAsync(NewUpdate(assetUrl: null));

            Assert.That(_ui.InfoMessages.Count, Is.EqualTo(1));
            Assert.That(_ui.InfoMessages[0], Does.Contain("no matching download"));
            Assert.That(_ui.OpenedUrls, Does.Contain("http://test.local/releases/v3.0.0"));
            Assert.That(_ui.ExitCount, Is.EqualTo(0));
        }

        [Test]
        public async Task DownloadAndApplyUpdateAsync_ProtectedLocation_No_DoesNotOpenPage()
        {
            UpdateFlow protectedFlow = new UpdateFlow(_service, _ui, () => false, _updatesDir);
            _ui.ConfirmResult = false;

            await protectedFlow.DownloadAndApplyUpdateAsync(NewUpdate());

            Assert.That(_ui.Confirmations.Count, Is.EqualTo(1));
            Assert.That(_ui.OpenedUrls.Count, Is.EqualTo(0));
            Assert.That(_ui.ExitCount, Is.EqualTo(0));
        }

        [Test]
        public async Task DownloadAndApplyUpdateAsync_ProtectedLocation_Yes_OpensPage()
        {
            UpdateFlow protectedFlow = new UpdateFlow(_service, _ui, () => false, _updatesDir);
            _ui.ConfirmResult = true;

            await protectedFlow.DownloadAndApplyUpdateAsync(NewUpdate());

            Assert.That(_ui.OpenedUrls, Does.Contain("http://test.local/releases/v3.0.0"));
            Assert.That(_ui.ExitCount, Is.EqualTo(0));
        }

        [Test]
        public async Task DownloadAndApplyUpdateAsync_DownloadFails_ShowsErrorAndNoExit()
        {
            Respond(HttpStatusCode.NotFound);
            UpdateInfo update = NewUpdate();

            await _flow.DownloadAndApplyUpdateAsync(update);

            Assert.That(_ui.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(_ui.ErrorMessages[0], Does.Contain("Could not download the update"));
            Assert.That(_ui.Statuses, Does.Contain("Update download failed"));
            Assert.That(_ui.ExitCount, Is.EqualTo(0));
            Assert.That(File.Exists(Path.Combine(_updatesDir, update.AssetName)), Is.False);
        }

        [Test]
        public async Task DownloadAndApplyUpdateAsync_DownloadSucceeds_LaunchesUpdaterAndExits()
        {
            UpdateInfo update = NewUpdate();

            await _flow.DownloadAndApplyUpdateAsync(update);

            string destination = Path.Combine(_updatesDir, update.AssetName);
            Assert.That(File.Exists(destination), Is.True);
            Assert.That(_service.LastLaunchedPath, Is.EqualTo(destination));
            Assert.That(_ui.ExitCount, Is.EqualTo(1));
            Assert.That(_ui.Progress, Does.Contain(100));
        }

        [Test]
        public async Task DownloadAndApplyUpdateAsync_FileAlreadyExists_SkipsDownload()
        {
            UpdateInfo update = NewUpdate();
            string destination = Path.Combine(_updatesDir, update.AssetName);
            File.WriteAllText(destination, "existing");

            await _flow.DownloadAndApplyUpdateAsync(update);

            Assert.That(_ui.Statuses, Does.Not.Contain("Downloading update..."));
            Assert.That(_ui.Statuses, Does.Contain("Applying update..."));
            Assert.That(_service.LastLaunchedPath, Is.EqualTo(destination));
            Assert.That(_ui.ExitCount, Is.EqualTo(1));
        }

        [Test]
        public async Task DownloadAndApplyUpdateAsync_LaunchFails_ShowsErrorAndOpensPage()
        {
            _service.LaunchResult = false;

            await _flow.DownloadAndApplyUpdateAsync(NewUpdate());

            Assert.That(_ui.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(_ui.ErrorMessages[0], Does.Contain("Could not apply the update automatically"));
            Assert.That(_ui.OpenedUrls, Does.Contain("http://test.local/releases/v3.0.0"));
            Assert.That(_ui.ExitCount, Is.EqualTo(0));
            Assert.That(_ui.Statuses[_ui.Statuses.Count - 1], Is.EqualTo("Ready"));
        }
    }
}
