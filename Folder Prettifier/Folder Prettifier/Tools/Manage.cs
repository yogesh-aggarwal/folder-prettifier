using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Folder_Prettifier.Tools
{
    enum AutoNameManipulation
    {
        None, Capitalize, AntiCapitalize, AllCapital, AllSmall
    }

    enum AutoCategorizeFiles
    {
        Extensions, Date, None
    }

    class NameAppend
    {
        public string AtStart = "";
        public string AtEnd = "";
    }

    class ManageFolderManageData
    {
        public static bool isNameManipulation = true;
        public static AutoNameManipulation nameManipulation = AutoNameManipulation.Capitalize;

        public static NameAppend nameAppend = new NameAppend();

        public static bool isCategorize = true;
        public static AutoCategorizeFiles categorizeFiles = AutoCategorizeFiles.Extensions;
    }

    class Manage
    {
        static string currentDirectory = "";

        private static string PrettifyName(string file)
        {
            // Prepare name data
            string[] fileNameWords = file.Split('.');
            string fileExtension = fileNameWords[^1];
            Array.Resize(ref fileNameWords, fileNameWords.Length - 1);
            string fileName = String.Join('.', fileNameWords);

            string newFileName = fileName;

            // Capitalize
            if (ManageFolderManageData.nameManipulation == AutoNameManipulation.Capitalize)
            {
                newFileName = newFileName[0].ToString().ToUpper() + newFileName.Substring(1).ToLower();
            }
            /*
            if (isReplaceWord.Checked)
            {
                newFileName = newFileName.Replace(replaceWord.Text, withWord.Text);
            }
            */

            // Append at Start & End
            newFileName = ManageFolderManageData.nameAppend.AtStart + newFileName + ManageFolderManageData.nameAppend.AtEnd;
            string newFile = $"{newFileName}.{fileExtension}";
            string newFilePath = $@"{Directory.GetCurrentDirectory()}\{newFile}";

            // Rename the file
            File.Move(file, newFilePath);
            return newFile;
        }

        private static void CategorizeFile(string file)
        {
            try
            {
                string[] fileNameWords = file.Split('.');
                string folderName = Data.catalog[fileNameWords[^1].ToLower()].folderName;

                Directory.CreateDirectory(folderName);
                string newFilePath = $@"{folderName}\{file}";

                if (File.Exists(newFilePath))
                {
                    byte[] fileB = File.ReadAllBytes(file);
                    byte[] newfileB = File.ReadAllBytes(newFilePath);
                    bool equal = fileB.SequenceEqual(newfileB);

                    if (File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(newFilePath)))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    File.Move(file, newFilePath);
                }

            }
            catch { }
        }

        private static void PrettifyFileContent(string file)
        {
            string[] punctuations = { ",", ".", "?" };
            string content = File.ReadAllText(file);
        }

        /*
        private static void RenameFolder()
        {
            List<string> locationWords = location.Text.Split('\\').ToList();
            locationWords[locationWords.Count - 1] = renameTo.Text;
            string newPath = String.Join("\\", locationWords);

            if (location.Text != newPath)
            {
                Directory.SetCurrentDirectory(userPath);

                if (Directory.Exists(newPath))
                {
                    DialogResult resut = MessageBox.Show("Folder cannot be renamed as another folder with the new name found. If you proceed, the contents of the new folder will be deleted & files from the current folder will be moved to the new folder!\n\nDo you want to proceed?", "Folder Conflict!", MessageBoxButtons.YesNo);
                    if (resut == DialogResult.Yes)
                    {
                        Directory.Delete(newPath);
                    }
                    else
                    {
                        return;
                    }
                }

                Directory.Move(location.Text, newPath);
                Directory.SetCurrentDirectory(newPath);
                location.Text = newPath;
            }
        }
        */

        private static void ProcessFile(string file)
        {
            if (ManageFolderManageData.nameManipulation != AutoNameManipulation.None)
            {
                file = PrettifyName(file);
            }
            //if (isPrettifyFile.Checked) PrettifyFileContent(file);
            if (ManageFolderManageData.isCategorize) CategorizeFile(file);

            //RenameFolder();
        }

        public static void Start(List<string> folders)
        {
            foreach (string folderPath in folders)
            {
                try
                {
                    Directory.SetCurrentDirectory(folderPath);
                    currentDirectory = Directory.GetCurrentDirectory();
                    string[] files = Directory.GetFiles(folderPath);

                    int totalFiles = files.Length;
                    int processedFiles = 0;

                    foreach (string file in files)
                    {
                        string fileName = file.Split('\\')[^1];
                        ProcessFile(fileName);
                        processedFiles++;
                    }
                }
                catch { }
            }
        }
    }
}
