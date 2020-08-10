using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FolderPrettifier
{
    public partial class Main : Form
    {
        dynamic extensions;
        dynamic folders = new List<string>();
        string userPath;
        const string InternetCheckUrl = "http://google.com/generate_204";
        const string CatalogUrl = "https://raw.githubusercontent.com/yogesh-aggarwal/folder-prettifier/master/catalog.json";
        string CatalogFilePath = $@"{Path.GetTempPath()}folder_prettifier_catalog.json";

        public Main()
        {
            InitializeComponent();

            setCurrentPath();

            FetchCatalog();
        }

        private void setCurrentPath()
        {
            string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            if (Environment.OSVersion.Version.Major >= 6)
            {
                path = Directory.GetParent(path).ToString();
            }

            userPath = path;

            location.Text = path + @"\Downloads";
            location.Text = @"E:\Compressions - Copy";
            Directory.SetCurrentDirectory(location.Text);
        }

        public static bool CheckInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead(InternetCheckUrl))
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
                status.Text = "Checking online...";
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(CatalogUrl))
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
            else
            {
                try
                {
                    status.Text = "Reading catalog from cache...";
                    using (StreamReader sr = File.OpenText(CatalogFilePath))
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
                    Console.WriteLine("No Catalog!!!");
                }
            }

            if (result == "")
            {
                status.Text = "No catalog! Can't proceed";
                return;
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

        private void chooseLocation_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                location.Text = folderDialog.SelectedPath;
            }
        }

        private void location_TextChanged(object sender, EventArgs e)
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

        private void isPrettifyName_CheckedChanged(object sender, EventArgs e)
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

        private void isPrettifyFile_CheckedChanged(object sender, EventArgs e)
        {
            isHandleSpacing.Enabled = isPrettifyFile.Enabled && isPrettifyFile.Checked;
        }

        private void isReplaceWord_CheckedChanged(object sender, EventArgs e)
        {
            replaceWordLabel.Enabled = isReplaceWord.Checked;
            replaceWord.Enabled = isReplaceWord.Checked;

            withWordLabel.Enabled = isReplaceWord.Checked;
            withWord.Enabled = isReplaceWord.Checked;
        }

        private void isNameWith_CheckedChanged(object sender, EventArgs e)
        {
            nameStartsWithLabel.Enabled = isNameWith.Checked;
            nameStartsWith.Enabled = isNameWith.Checked;

            nameEndsWithLabel.Enabled = isNameWith.Checked;
            nameEndsWith.Enabled = isNameWith.Checked;
        }

        private void showPathError(dynamic element)
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

        private void nameStartsWith_TextChanged(object sender, EventArgs e)
        {
            showPathError(nameStartsWith);
        }

        private void nameEndsWith_TextChanged(object sender, EventArgs e)
        {
            showPathError(nameEndsWith);
        }

        private void replaceWord_TextChanged(object sender, EventArgs e)
        {
            showPathError(replaceWord);
        }

        private void withWord_TextChanged(object sender, EventArgs e)
        {
            showPathError(withWord);
        }

        private void renameTo_TextChanged(object sender, EventArgs e)
        {
            showPathError(renameTo);
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
                Console.WriteLine(newFileName);
            }

            File.Move(file, $@"{backPath}\{newFileName}");
            return $@"{backPath}\{newFileName}";
        }

        private void CategorizeFile(string file)
        {
            int folderIndex = extensions[file.Split('.').Last()];

            string folderName = folders[folderIndex];
            Directory.CreateDirectory(folderName);
            File.Move(file, $"{folderName}\\{file.Split('\\').Last()}");
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
            int totalFiles = int.Parse(totalFilesCount.Text);
            int processedFiles = 0;

            string[] files = Directory.GetFiles(location.Text);


            foreach (string file in files)
            {
                ProcessFile(file);
                processedFiles++;

                progressBar.Value = processedFiles / totalFiles * 100;
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            DialogResult proceed = MessageBox.Show("Now, several operations will be performed on your folder. During the process, DON'T CLOSE THE APPLICATION in any manner. If you do so, there're high chances of data corruption. It's also recommeded not to work on this folder/subfolders.\n\nDo you want to proceed?", "ATTENTION!", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (proceed == DialogResult.Yes)
            {
                StartProcess();

                if (isOpenFolder.Checked)
                {
                    Process.Start("explorer.exe", location.Text);
                }

                MessageBox.Show("All the prettification is done & your folder looks clean & managed now!", "Enjoy!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void aboutBtn_Click(object sender, EventArgs e)
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
