using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace FolderPrettifier
{
    public partial class Main : Form
    {
        private readonly RemoteFileFetcher _remoteFetcher = new RemoteFileFetcher();
        private readonly CatalogService _catalogService;
        private readonly UpdateService _updateService = new UpdateService();

        Dictionary<string, string> _extensions;
        string _defaultFolder;
        bool _catalogLoaded;
        bool _updateCheckInProgress;
        string currentFolder = "";

        public Main(string currentFolder = "")
        {
            InitializeComponent();

            this.currentFolder = currentFolder;

            _catalogService = new CatalogService(_remoteFetcher,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Folder Prettifier", Data.CatalogCacheDir),
                Data.CatalogUrl,
                Data.VersionsFileName,
                () => Data.BasicCatalog);

            startBtn.Enabled = false;

            SetCurrentPath();

            FetchCatalog();
            _ = CheckForUpdateAsync(true);
        }

        private void SetCurrentPath()
        {
            try
            {
                if (currentFolder.Length != 0)
                {
                    location.Text = currentFolder;
                }
                else
                {
                    string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    location.Text = Path.Combine(userProfile, "Downloads");
                }

                if (!Directory.Exists(location.Text))
                {
                    Directory.CreateDirectory(location.Text);
                }
            }
            catch
            {
                string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                location.Text = Path.Combine(userProfile, "Downloads");

                if (!Directory.Exists(location.Text))
                {
                    Directory.CreateDirectory(location.Text);
                }
            }
        }

        private async void FetchCatalog()
        {
            status.Text = "Fetching Catalog...";
            startBtn.Enabled = false;
            updateCatalogBtn.Enabled = false;
            progressBar.Value = 0;

            Version appVersion = GetAppVersion();

            bool online = await _remoteFetcher.CheckAsync(Data.InternetCheckUrl, TimeSpan.FromSeconds(3));
            progressBar.Value = 30;

            CatalogLoadOutcome outcome = await _catalogService.LoadAsync(appVersion, online, s => status.Text = s);

            if (outcome.UpdateRequired)
            {
                RequireUpdate(appVersion);
                return;
            }

            Catalog catalog = outcome.Catalog;
            if (catalog == null)
            {
                status.Text = "No catalog! Can't proceed";
                updateCatalogBtn.Enabled = true;
                return;
            }

            _extensions = catalog.BuildExtensionMap();
            _defaultFolder = catalog.DefaultFolder;
            _catalogLoaded = true;

            startBtn.Enabled = true;
            updateCatalogBtn.Enabled = true;

            progressBar.Value = 100;

            await Task.Delay(500);
            status.Text = "Ready";
            progressBar.Value = 0;
        }

        private static Version GetAppVersion()
        {
            Version version;
            return Version.TryParse(Application.ProductVersion, out version) ? version : new Version(0, 0);
        }

        private void RequireUpdate(Version appVersion)
        {
            string message = "Your installed version of Folder Prettifier (" + appVersion + ") is no longer supported by the current catalog system. "
                + "The catalog used by this application requires a newer version.\n\n"
                + "Please download and install the latest version from:\n"
                + "https://github.com/yogesh-aggarwal/folder-prettifier\n\n"
                + "After updating, please restart the application.";
            MessageBox.Show(message, "Update Required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            status.Text = "Update required";
            updateCatalogBtn.Enabled = true;
            progressBar.Value = 0;
        }

        private void ChooseLocation_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                location.Text = folderDialog.SelectedPath;
            }
        }

        private void Location_TextChanged(object sender, EventArgs e)
        {
            try
            {
                totalFilesCount.Text = Directory.GetFiles(location.Text).Length.ToString();
                renameTo.Text = Path.GetFileName(location.Text);
            }
            catch
            {
                totalFilesCount.Text = "0";
                renameTo.Text = "";
            }
        }

        private void IsPrettifyName_CheckedChanged(object sender, EventArgs e)
        {
            isCapitalizeName.Enabled = isPrettifyName.Checked;

            isReplaceWord.Enabled = isPrettifyName.Checked;
            replaceWordLabel.Enabled = isPrettifyName.Checked && isReplaceWord.Checked;
            replaceWord.Enabled = isPrettifyName.Checked && isReplaceWord.Checked;
            withWordLabel.Enabled = isPrettifyName.Checked && isReplaceWord.Checked;
            withWord.Enabled = isPrettifyName.Checked && isReplaceWord.Checked;

            isNameWith.Enabled = isPrettifyName.Checked;
            nameStartsWithLabel.Enabled = isPrettifyName.Checked && isNameWith.Checked;
            nameStartsWith.Enabled = isPrettifyName.Checked && isNameWith.Checked;
            nameEndsWithLabel.Enabled = isPrettifyName.Checked && isNameWith.Checked;
            nameEndsWith.Enabled = isPrettifyName.Checked && isNameWith.Checked;
        }

        private void IsReplaceWord_CheckedChanged(object sender, EventArgs e)
        {
            replaceWordLabel.Enabled = isReplaceWord.Checked;
            replaceWord.Enabled = isReplaceWord.Checked;

            withWordLabel.Enabled = isReplaceWord.Checked;
            withWord.Enabled = isReplaceWord.Checked;
        }

        private void IsNameWith_CheckedChanged(object sender, EventArgs e)
        {
            nameStartsWithLabel.Enabled = isNameWith.Checked;
            nameStartsWith.Enabled = isNameWith.Checked;

            nameEndsWithLabel.Enabled = isNameWith.Checked;
            nameEndsWith.Enabled = isNameWith.Checked;
        }

        private void ShowPathError(dynamic element)
        {
            string original = element.Text;
            string cleaned = FileNamePrettifier.Sanitize(original);

            if (cleaned != original)
            {
                MessageBox.Show("Some invalid characters have been removed from the name.", "Invalid name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                element.Text = cleaned;
            }
        }

        private void NameStartsWith_TextChanged(object sender, EventArgs e)
        {
            ShowPathError(nameStartsWith);
        }

        private void NameEndsWith_TextChanged(object sender, EventArgs e)
        {
            ShowPathError(nameEndsWith);
        }

        private void ReplaceWord_TextChanged(object sender, EventArgs e)
        {
            ShowPathError(replaceWord);
        }

        private void WithWord_TextChanged(object sender, EventArgs e)
        {
            ShowPathError(withWord);
        }

        private void RenameTo_TextChanged(object sender, EventArgs e)
        {
            ShowPathError(renameTo);
        }

        private void StartProcess()
        {
            string srcFolder = location.Text;

            string[] files = Directory.GetFiles(srcFolder);

            ProcessingOptions options = new ProcessingOptions();
            Invoke(new Action(() =>
            {
                options.PrettifyOn = isPrettifyName.Checked;
                options.CategorizeOn = isCategorizeFiles.Checked;
                options.Prettify = new PrettifyOptions
                {
                    Capitalize = isCapitalizeName.Checked,
                    Replace = isReplaceWord.Checked,
                    ReplaceFrom = replaceWord.Text,
                    ReplaceTo = withWord.Text,
                    UseNameWith = isNameWith.Checked,
                    Prefix = nameStartsWith.Text,
                    Suffix = nameEndsWith.Text
                };
            }));

            int totalFiles = files.Length;
            int processedFiles = 0;

            FileProcessingResult result = FileProcessor.ProcessFiles(srcFolder, files, options, _extensions, _defaultFolder,
                file =>
                {
                    Invoke(new Action(() => status.Text = file));
                    processedFiles++;
                    int progress = totalFiles > 0 ? processedFiles * 100 / totalFiles : 0;
                    Invoke(new Action(() => progressBar.Value = progress));
                });

            string renameTarget = "";
            Invoke(new Action(() => renameTarget = renameTo.Text));

            RenamePlan plan = RenamePlanner.Plan(srcFolder, renameTarget);
            if (plan.IsRename)
            {
                if (plan.Conflict)
                {
                    bool userSaidYes = false;
                    Invoke(new Action(() =>
                    {
                        DialogResult res = MessageBox.Show("Folder cannot be renamed as another folder with the new name found. If you proceed, the contents of the new folder will be deleted & files from the current folder will be moved to the new folder!\n\nDo you want to proceed?", "Folder Conflict!", MessageBoxButtons.YesNo);
                        userSaidYes = res == DialogResult.Yes;
                    }));
                    if (userSaidYes)
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(plan.TargetPath,
                            Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                            Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        Invoke(new Action(() => status.Text = "Ready"));
                        return;
                    }
                }

                if (FileProcessor.ApplyRename(srcFolder, plan))
                {
                    Invoke(new Action(() => location.Text = plan.TargetPath));
                }
            }

            if (result.Errors.Count > 0)
            {
                string message = "The following files could not be processed:\n\n" + string.Join("\n", result.Errors);
                Invoke(new Action(() => MessageBox.Show(message, "Some files could not be processed", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
            }

            Invoke(new Action(() => status.Text = "Ready"));
        }

        private async void StartBtn_Click(object sender, EventArgs e)
        {
            if (!_catalogLoaded)
            {
                MessageBox.Show("Catalog is still loading. Please wait.", "Not Ready", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult proceed = MessageBox.Show("Now, several operations will be performed on your folder. During the process, DON'T CLOSE THE APPLICATION in any manner. If you do so, there're high chances of data corruption. It's also recommeded not to work on this folder/subfolders.\n\nDo you want to proceed?", "ATTENTION!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (proceed == DialogResult.Yes)
            {
                startBtn.Enabled = false;
                try
                {
                    await Task.Run(() => StartProcess());

                    if (isOpenFolder.Checked)
                    {
                        Process.Start("explorer.exe", location.Text);
                    }

                    Location_TextChanged(sender, e);

                    MessageBox.Show("All the prettification is done & your folder looks clean & managed now!", "Enjoy!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                finally
                {
                    startBtn.Enabled = true;
                    progressBar.Value = 0;
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog about = new AboutDialog();
            about.ShowDialog();
        }

        private void updateCatalogBtn_Click(object sender, EventArgs e)
        {
            FetchCatalog();
        }

        private async void checkForUpdatesBtn_Click(object sender, EventArgs e)
        {
            await CheckForUpdateAsync(false);
        }

        private async Task CheckForUpdateAsync(bool silent)
        {
            if (_updateCheckInProgress)
            {
                return;
            }

            _updateCheckInProgress = true;
            checkForUpdatesBtn.Enabled = false;

            status.Text = "Checking for updates...";
            progressBar.Value = 0;

            UpdateInfo update = null;
            try
            {
                update = await _updateService.CheckForUpdateAsync(GetAppVersion());
            }
            finally
            {
                _updateCheckInProgress = false;
            }

            if (update == null)
            {
                checkForUpdatesBtn.Enabled = true;
                status.Text = "Ready";
                progressBar.Value = 0;
                if (!silent)
                {
                    MessageBox.Show("You're running the latest version of Folder Prettifier.", "Up to date", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            if (silent && !ShouldPromptForUpdate())
            {
                checkForUpdatesBtn.Enabled = true;
                status.Text = "Ready";
                return;
            }

            string notes = string.IsNullOrWhiteSpace(update.ReleaseNotes) ? "" : "\n\nWhat's new:\n" + update.ReleaseNotes;
            DialogResult answer = MessageBox.Show(
                "A new version of Folder Prettifier is available: " + update.Version + "\n" +
                "Your current version: " + GetAppVersion() + notes +
                "\n\nDo you want to download and install it now?",
                "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                RememberUpdatePrompt();
                checkForUpdatesBtn.Enabled = true;
                status.Text = "Ready";
                return;
            }

            await DownloadAndApplyUpdateAsync(update);
            checkForUpdatesBtn.Enabled = true;
            status.Text = "Ready";
        }

        private async Task DownloadAndApplyUpdateAsync(UpdateInfo update)
        {
            if (string.IsNullOrEmpty(update.AssetUrl))
            {
                MessageBox.Show(
                    "An update is available but no matching download could be found for this version of Windows.\n\n" +
                    "Please download the latest version from the release page.",
                    "Manual Update Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start(update.ReleasePageUrl);
                return;
            }

            if (!UpdateService.CanUpdateInPlace())
            {
                DialogResult openPage = MessageBox.Show(
                    "Folder Prettifier cannot update itself because it is installed in a protected location.\n\n" +
                    "Please download the latest version manually. Open the release page now?",
                    "Manual Update Required", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (openPage == DialogResult.Yes)
                {
                    Process.Start(update.ReleasePageUrl);
                }
                return;
            }

            string updatesDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Folder Prettifier", "updates");
            string destination = Path.Combine(updatesDir, update.AssetName);

            if (!File.Exists(destination))
            {
                status.Text = "Downloading update...";
                progressBar.Value = 0;
                bool downloaded = await _updateService.DownloadAsync(update, destination,
                    new Progress<int>(p => progressBar.Value = p));
                if (!downloaded)
                {
                    status.Text = "Update download failed";
                    MessageBox.Show("Could not download the update. Please try again later.", "Download Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            status.Text = "Applying update...";
            if (_updateService.LaunchUpdater(destination))
            {
                Application.Exit();
            }
            else
            {
                status.Text = "Ready";
                MessageBox.Show(
                    "Could not apply the update automatically. Please download the latest version from the release page.",
                    "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.Start(update.ReleasePageUrl);
            }
        }

        private static string UpdatePromptFile
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Folder Prettifier", "last-update-prompt.txt");
            }
        }

        private static bool ShouldPromptForUpdate()
        {
            try
            {
                if (!File.Exists(UpdatePromptFile))
                {
                    return true;
                }
                DateTime lastPrompt;
                if (!DateTime.TryParse(File.ReadAllText(UpdatePromptFile).Trim(), out lastPrompt))
                {
                    return true;
                }
                return (DateTime.Now - lastPrompt).TotalDays >= 7;
            }
            catch
            {
                return true;
            }
        }

        private static void RememberUpdatePrompt()
        {
            try
            {
                File.WriteAllText(UpdatePromptFile, DateTime.Now.ToString("o"));
            }
            catch
            {
            }
        }
    }
}
