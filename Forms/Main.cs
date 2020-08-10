using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FolderPrettifier
{
    public partial class Main : Form
    {
        dynamic extensions;
        dynamic folders = new List<string>();
        string userPath;

        public Main()
        {
            InitializeComponent();

            SetCurrentPath();

            FetchCatalog();
        }

        private void SetCurrentPath()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                path = Directory.GetParent(path).ToString();
            }

            userPath = path;

            location.Text = path + @"\Downloads";
            Directory.SetCurrentDirectory(location.Text);
        }

        public static bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead(Data.InternetCheckUrl))
                {
                    return true;
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

            if (CheckInternetConnection())
            {
                try
                {
                    status.Text = "Checking online...";
                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage response = await client.GetAsync(Data.CatalogUrl))
                    using (HttpContent content = response.Content)
                    {
                        progressBar.Value = 50;

                        status.Text = "Caching latest catalog...";
                        result = await content.ReadAsStringAsync();
                        using (StreamWriter sw = File.CreateText($@"{Path.GetTempPath()}folder_prettifier_catalog.json"))
                        {
                            sw.WriteLine(result);
                        }
                    }

                }
                catch
                {

                }
            }
            else
            {
                try
                {
                    status.Text = "Reading catalog from cache...";
                    using (StreamReader sr = File.OpenText($@"{Path.GetTempPath()}{Data.CacheFileName}"))
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

                }
            }

            if (result == "")
            {
                status.Text = "Using basic catalog...";
                result = Data.BasicCatalog;
            }

            dynamic obj = JsonConvert.DeserializeObject(result);
            extensions = obj.extensions;
            folders = obj.folders;

            progressBar.Value = 100;

            await Task.Delay(2000);
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
                renameTo.Text = location.Text.Split('\\').Last();
                Directory.SetCurrentDirectory(location.Text);
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

        private void IsPrettifyFile_CheckedChanged(object sender, EventArgs e)
        {
            isHandleSpacing.Enabled = isPrettifyFile.Enabled && isPrettifyFile.Checked;
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
            char character = '0';

            if (element.Text.Contains(@"\"))
            {
                character = '\\';
            }

            if (element.Text.Contains(@"/"))
            {
                character = '/';
            }
            if (element.Text.Contains(@"|"))
            {
                character = '|';
            }
            if (element.Text.Contains(@"?"))
            {
                character = '?';
            }
            if (element.Text.Contains(@"*"))
            {
                character = '*';

            }
            if (element.Text.Contains(@"<"))
            {
                character = '<';
            }
            if (element.Text.Contains(@">"))
            {
                character = '>';
            }
            if (element.Text.Contains(@":"))
            {
                character = ':';
            }
            if (element.Text.Contains("\""))
            {
                character = '"';
            }

            if (character != '0')
            {
                MessageBox.Show($"Remove \"{character}\" from the name as it's not a valid file name in Windows.", "Invalid name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                element.Text = element.Text.Replace(character.ToString(), "");
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

        private string PrettifyName(string file)
        {
            string[] fileNameWords = file.Split('\\');
            string backPath = string.Join(@"\", fileNameWords.Take(fileNameWords.Length - 1));
            string fileName = fileNameWords[fileNameWords.Length - 1];
            string newFileName = fileName;

            if (isCapitalizeName.Checked)
            {
                newFileName = newFileName.First().ToString().ToUpper() + newFileName.Substring(1);
            }
            if (isReplaceWord.Checked)
            {
                newFileName = newFileName.Replace(replaceWord.Text, withWord.Text);
            }
            if (isNameWith.Checked)
            {
                string[] newFileNameWords = newFileName.Split('.');
                newFileName = nameStartsWith.Text + string.Join(".", newFileNameWords.Take(newFileNameWords.Length - 1)) + nameEndsWith.Text + "." + newFileNameWords.Last();
            }

            File.Move(file, $@"{backPath}\{newFileName}");
            return $@"{backPath}\{newFileName}";
        }

        private void CategorizeFile(string file)
        {
            try
            {
                int folderIndex = extensions[file.Split('.').Last()];

                string folderName = folders[folderIndex];
                Directory.CreateDirectory(folderName);
                File.Move(file, $"{folderName}\\{file.Split('\\').Last()}");

            }
            catch
            {

            }
        }

        private void PrettifyFileContent(string file)
        {
            Console.WriteLine("Prettify File");
        }

        private void RenameFolder()
        {
            string newName = location.Text.Replace(location.Text.Split('\\').Last(), renameTo.Text);

            if (location.Text != newName)
            {
                Directory.SetCurrentDirectory(userPath);

                if (Directory.Exists(newName))
                {
                    DialogResult resut = MessageBox.Show("Folder cannot be renamed as another folder with the new name found. If you proceed, the contents of the new folder will be deleted & files from the current folder will be moved to the new folder!\n\nDo you want to proceed?", "Folder Conflict!", MessageBoxButtons.YesNo);
                    if (resut == DialogResult.Yes)
                    {
                        Directory.Delete(newName);
                    }
                    else
                    {
                        return;
                    }
                }

                Directory.Move(location.Text, newName);
                Directory.SetCurrentDirectory(newName);
                location.Text = newName;
            }
        }

        private void ProcessFile(string file)
        {
            if (isPrettifyName.Checked)
            {
                file = PrettifyName(file);
            }
            if (isPrettifyFile.Checked) PrettifyFileContent(file);
            if (isCategorizeFiles.Checked) CategorizeFile(file);

            RenameFolder();
        }

        private void StartProcess()
        {
            string[] files = Directory.GetFiles(location.Text);

            int totalFiles = files.Length;
            int processedFiles = 0;

            foreach (string file in files)
            {
                ProcessFile(file);
                processedFiles++;

                progressBar.Value = processedFiles / totalFiles * 100;
            }
        }

        private void StartBtn_Click(object sender, EventArgs e)
        {
            DialogResult proceed = MessageBox.Show("Now, several operations will be performed on your folder. During the process, DON'T CLOSE THE APPLICATION in any manner. If you do so, there're high chances of data corruption. It's also recommeded not to work on this folder/subfolders.\n\nDo you want to proceed?", "ATTENTION!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (proceed == DialogResult.Yes)
            {
                StartProcess();

                if (isOpenFolder.Checked)
                {
                    Process.Start("explorer.exe", location.Text);
                }

                Location_TextChanged(sender, e);

                MessageBox.Show("All the prettification is done & your folder looks clean & managed now!", "Enjoy!", MessageBoxButtons.OK, MessageBoxIcon.Information);

                progressBar.Value = 0;
            };
        }

        private void AboutBtn_Click(object sender, EventArgs e)
        {
            AboutDialog about = new AboutDialog();
            about.ShowDialog();
        }

        private void UpdateCatalogBtn_Click(object sender, EventArgs e)
        {
            FetchCatalog();
        }
    }
}
