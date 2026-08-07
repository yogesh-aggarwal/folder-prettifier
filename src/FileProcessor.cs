using System;
using System.Collections.Generic;
using System.IO;

namespace FolderPrettifier
{
    public class ProcessingOptions
    {
        public bool PrettifyOn { get; set; }

        public bool CategorizeOn { get; set; }

        public PrettifyOptions Prettify { get; set; }
    }

    public class FileProcessingResult
    {
        public int Processed { get; set; }

        public List<string> Errors { get; private set; }

        public FileProcessingResult()
        {
            Errors = new List<string>();
        }
    }

    public static class FileProcessor
    {
        public static FileProcessingResult ProcessFiles(
            string srcFolder,
            string[] files,
            ProcessingOptions options,
            IDictionary<string, string> extensionMap,
            string defaultFolder,
            Action<string> onFile = null)
        {
            FileProcessingResult result = new FileProcessingResult();
            if (files == null) return result;

            foreach (string file in files)
            {
                if (onFile != null) onFile(file);

                string currentFile = file;

                if (options.PrettifyOn)
                {
                    try
                    {
                        string backPath = Path.GetDirectoryName(currentFile);
                        string fileName = Path.GetFileName(currentFile);
                        string newFileName = FileNamePrettifier.Prettify(fileName, options.Prettify);
                        string dest = Path.Combine(backPath, newFileName);
                        File.Move(currentFile, dest);
                        currentFile = dest;
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add("Failed to prettify: " + Path.GetFileName(currentFile) + " (" + ex.Message + ")");
                        continue;
                    }
                }

                if (options.CategorizeOn)
                {
                    try
                    {
                        string catFolderName = FileCategorizer.CategoryFor(currentFile, extensionMap, defaultFolder);
                        string destDir = Path.Combine(srcFolder, catFolderName);
                        Directory.CreateDirectory(destDir);
                        string destPath = FileCategorizer.ResolveDestinationPath(destDir, Path.GetFileName(currentFile), File.Exists);
                        File.Move(currentFile, destPath);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add("Failed to categorize: " + Path.GetFileName(currentFile) + " (" + ex.Message + ")");
                    }
                }

                result.Processed++;
            }

            return result;
        }

        public static bool ApplyRename(string srcFolder, RenamePlan plan)
        {
            if (plan == null || !plan.IsRename) return false;

            Directory.Move(srcFolder, plan.TargetPath);
            return true;
        }
    }
}
