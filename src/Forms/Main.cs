using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace FolderPrettifier
{
    public partial class Main : Form
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        dynamic extensions;
        bool _catalogLoaded;
        string currentFolder = "";

        public Main(string currentFolder = "")
        {
            InitializeComponent();

            this.currentFolder = currentFolder;

            startBtn.Enabled = false;

            SetCurrentPath();

            FetchCatalog();
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

        private async Task<bool> CheckInternetConnectionAsync()
        {
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
                using (var response = await _httpClient.GetAsync(Data.InternetCheckUrl, cts.Token))
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        private async void FetchCatalog()
        {
            status.Text = "Fetching Catalog...";
            startBtn.Enabled = false;
            updateCatalogBtn.Enabled = false;
            progressBar.Value = 0;

            string result = "";

            progressBar.Value = 30;

            if (await CheckInternetConnectionAsync())
            {
                try
                {
                    status.Text = "Checking online...";
                    using (HttpResponseMessage response = await _httpClient.GetAsync(Data.CatalogUrl))
                    using (HttpContent content = response.Content)
                    {

                        progressBar.Value = 50;
                        status.Text = "Caching latest catalog...";
                        result = await content.ReadAsStringAsync();
                        using (StreamWriter sw = File.CreateText(Path.Combine(Path.GetTempPath(), Data.CacheFileName)))
                        {
                            sw.WriteLine(result);
                        }
                    }
                }
                catch
                {
                    status.Text = "Failed to fetch online catalog, trying cache...";
                }
            }
            else
            {
                try
                {
                    status.Text = "Reading catalog from cache...";
                    using (StreamReader sr = File.OpenText(Path.Combine(Path.GetTempPath(), Data.CacheFileName)))
                    {
                        string s = "";
                        while ((s = sr.ReadLine()) != null)
                        {
                            result += s;
                        }
                    }
                }
                catch
                {
                    status.Text = "No cached catalog found, using embedded...";
                }
            }

            if (result == "")
            {
                status.Text = "Using basic catalog...";
                result = Data.BasicCatalog;
            }

            try
            {
                extensions = JsonConvert.DeserializeObject(result);
                _catalogLoaded = true;
            }
            catch
            {
                status.Text = "No catalog! Can't proceed";
                return;
            }

            startBtn.Enabled = true;

            progressBar.Value = 100;

            await Task.Delay(500);
            status.Text = "Ready";
            startBtn.Enabled = true;
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
            char[] invalidChars = Path.GetInvalidFileNameChars();
            string original = element.Text;
            string cleaned = new string(original.Where(c => !invalidChars.Contains(c)).ToArray());

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
            int totalFiles = files.Length;
            int processedFiles = 0;

            bool prettifyOn = false, categorizeOn = false;
            bool capitalizeOn = false, replaceOn = false, nameWithOn = false;
            string replaceFrom = "", replaceTo = "", namePrefix = "", nameSuffix = "";
            Invoke(new Action(() =>
            {
                prettifyOn = isPrettifyName.Checked;
                categorizeOn = isCategorizeFiles.Checked;
                capitalizeOn = isCapitalizeName.Checked;
                replaceOn = isReplaceWord.Checked;
                replaceFrom = replaceWord.Text;
                replaceTo = withWord.Text;
                nameWithOn = isNameWith.Checked;
                namePrefix = nameStartsWith.Text;
                nameSuffix = nameEndsWith.Text;
            }));

            foreach (string file in files)
            {
                string currentFile = file;
                Invoke(new Action(() => status.Text = currentFile));

                if (prettifyOn)
                {
                    string backPath = Path.GetDirectoryName(currentFile);
                    string fileName = Path.GetFileName(currentFile);
                    string newFileName = fileName;

                    if (capitalizeOn && newFileName.Length > 0)
                        newFileName = char.ToUpper(newFileName[0]) + newFileName.Substring(1);

                    if (replaceOn)
                        newFileName = newFileName.Replace(replaceFrom, replaceTo);

                    if (nameWithOn)
                    {
                        int dotIndex = newFileName.LastIndexOf('.');
                        if (dotIndex > 0)
                        {
                            string namePart = newFileName.Substring(0, dotIndex);
                            string extPart = newFileName.Substring(dotIndex);
                            newFileName = namePrefix + namePart + nameSuffix + extPart;
                        }
                        else
                        {
                            newFileName = namePrefix + newFileName + nameSuffix;
                        }
                    }

                    string dest = Path.Combine(backPath, newFileName);
                    File.Move(currentFile, dest);
                    currentFile = dest;
                }

                if (categorizeOn)
                {
                    try
                    {
                        string ext = Path.GetExtension(currentFile).TrimStart('.').ToLower();
                        if (!string.IsNullOrEmpty(ext) && extensions[ext] != null)
                        {
                            string catFolderName = extensions[ext]["folderName"].ToString();
                            string catFileName = Path.GetFileName(currentFile);
                            string destDir = Path.Combine(srcFolder, catFolderName);
                            Directory.CreateDirectory(destDir);

                            string destPath = Path.Combine(destDir, catFileName);
                            if (File.Exists(destPath))
                            {
                                string nameNoExt = Path.GetFileNameWithoutExtension(catFileName);
                                string extOnly = Path.GetExtension(catFileName);
                                int suffix = 1;
                                do
                                {
                                    destPath = Path.Combine(destDir, $"{nameNoExt} ({suffix}){extOnly}");
                                    suffix++;
                                } while (File.Exists(destPath));
                            }

                            File.Move(currentFile, destPath);
                        }
                    }
                    catch
                    {
                        Invoke(new Action(() => status.Text = $"Failed to categorize: {Path.GetFileName(currentFile)}"));
                    }
                }

                processedFiles++;
                int progress = totalFiles > 0 ? processedFiles * 100 / totalFiles : 0;
                Invoke(new Action(() => progressBar.Value = progress));
            }

            string renameTarget = "";
            Invoke(new Action(() => renameTarget = renameTo.Text));
            if (renameTarget.Length > 0)
            {
                string parentDir = Path.GetDirectoryName(srcFolder);
                string newName = Path.Combine(parentDir, renameTarget);

                if (srcFolder != newName)
                {
                    bool userSaidYes = false;
                    if (Directory.Exists(newName))
                    {
                        Invoke(new Action(() =>
                        {
                            DialogResult res = MessageBox.Show("Folder cannot be renamed as another folder with the new name found. If you proceed, the contents of the new folder will be deleted & files from the current folder will be moved to the new folder!\n\nDo you want to proceed?", "Folder Conflict!", MessageBoxButtons.YesNo);
                            userSaidYes = res == DialogResult.Yes;
                        }));
                        if (userSaidYes)
                        {
                            Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(newName,
                                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                        }
                        else
                        {
                            Invoke(new Action(() => status.Text = "Ready"));
                            return;
                        }
                    }

                    Directory.Move(srcFolder, newName);
                    Invoke(new Action(() => location.Text = newName));
                }
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
    }
}
