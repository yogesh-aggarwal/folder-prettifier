using System;
using System.IO;
using System.Threading.Tasks;

namespace FolderPrettifier
{
    public interface IUpdateUi
    {
        void SetStatus(string text);
        void SetProgress(int value);
        void SetCheckEnabled(bool enabled);
        void ShowInfo(string text, string title);
        void ShowError(string text, string title);
        bool Confirm(string text, string title);
        void OpenUrl(string url);
        void Exit();
    }

    public class UpdateFlow
    {
        private readonly UpdateService _updateService;
        private readonly IUpdateUi _ui;
        private readonly Func<bool> _canUpdateInPlace;
        private readonly string _updatesDir;
        private bool _checkInProgress;

        public UpdateFlow(UpdateService updateService, IUpdateUi ui,
            Func<bool> canUpdateInPlace = null, string updatesDir = null)
        {
            _updateService = updateService;
            _ui = ui;
            _canUpdateInPlace = canUpdateInPlace ?? UpdateService.CanUpdateInPlace;
            _updatesDir = updatesDir ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Folder Prettifier", "updates");
        }

        public async Task CheckForUpdateAsync(bool silent, Version appVersion)
        {
            if (_checkInProgress)
            {
                return;
            }

            _checkInProgress = true;
            _ui.SetCheckEnabled(false);

            _ui.SetStatus("Checking for updates...");
            _ui.SetProgress(0);

            UpdateInfo update = null;
            try
            {
                update = await _updateService.CheckForUpdateAsync(appVersion);
            }
            finally
            {
                _checkInProgress = false;
            }

            if (update == null)
            {
                _ui.SetCheckEnabled(true);
                _ui.SetStatus("Ready");
                _ui.SetProgress(0);
                if (!silent)
                {
                    _ui.ShowInfo("You're running the latest version of Folder Prettifier.", "Up to date");
                }
                return;
            }

            if (silent)
            {
                // Startup check is non-intrusive: only hint in the status bar,
                // never pop a dialog (which would also block UI automation).
                _ui.SetCheckEnabled(true);
                _ui.SetStatus("Update available: " + update.Version + " (Check for Updates menu)");
                _ui.SetProgress(0);
                return;
            }

            string notes = string.IsNullOrWhiteSpace(update.ReleaseNotes) ? "" : "\n\nWhat's new:\n" + update.ReleaseNotes;
            bool downloadNow = _ui.Confirm(
                "A new version of Folder Prettifier is available: " + update.Version + "\n" +
                "Your current version: " + appVersion + notes +
                "\n\nDo you want to download and install it now?",
                "Update Available");

            if (!downloadNow)
            {
                _ui.SetCheckEnabled(true);
                _ui.SetStatus("Ready");
                return;
            }

            await DownloadAndApplyUpdateAsync(update);
            _ui.SetCheckEnabled(true);
            _ui.SetStatus("Ready");
        }

        public async Task DownloadAndApplyUpdateAsync(UpdateInfo update)
        {
            if (string.IsNullOrEmpty(update.AssetUrl))
            {
                _ui.ShowInfo(
                    "An update is available but no matching download could be found for this version of Windows.\n\n" +
                    "Please download the latest version from the release page.",
                    "Manual Update Required");
                _ui.OpenUrl(update.ReleasePageUrl);
                return;
            }

            if (!_canUpdateInPlace())
            {
                bool openPage = _ui.Confirm(
                    "Folder Prettifier cannot update itself because it is installed in a protected location.\n\n" +
                    "Please download the latest version manually. Open the release page now?",
                    "Manual Update Required");
                if (openPage)
                {
                    _ui.OpenUrl(update.ReleasePageUrl);
                }
                return;
            }

            string destination = Path.Combine(_updatesDir, update.AssetName);

            if (!File.Exists(destination))
            {
                _ui.SetStatus("Downloading update...");
                _ui.SetProgress(0);
                bool downloaded = await _updateService.DownloadAsync(update, destination,
                    new Progress<int>(p => _ui.SetProgress(p)));
                if (!downloaded)
                {
                    _ui.SetStatus("Update download failed");
                    _ui.ShowError("Could not download the update. Please try again later.", "Download Failed");
                    return;
                }
            }

            _ui.SetStatus("Applying update...");
            if (_updateService.LaunchUpdater(destination))
            {
                _ui.Exit();
            }
            else
            {
                _ui.SetStatus("Ready");
                _ui.ShowError(
                    "Could not apply the update automatically. Please download the latest version from the release page.",
                    "Update Failed");
                _ui.OpenUrl(update.ReleasePageUrl);
            }
        }
    }
}
