using System;
using System.Collections.Generic;
using System.IO;

namespace FolderPrettifier
{
    public static class FileCategorizer
    {
        public static string CategoryFor(string filePath, IDictionary<string, string> extensionMap, string defaultFolder)
        {
            string ext = Path.GetExtension(filePath).TrimStart('.').ToLowerInvariant();
            string folder;
            if (!extensionMap.TryGetValue(ext, out folder))
            {
                folder = defaultFolder;
            }
            return folder;
        }

        public static string ResolveDestinationPath(string destDir, string fileName, Func<string, bool> pathExists)
        {
            if (pathExists == null) throw new ArgumentNullException("pathExists");

            string destPath = Path.Combine(destDir, fileName);
            if (!pathExists(destPath))
            {
                return destPath;
            }

            string nameNoExt = Path.GetFileNameWithoutExtension(fileName);
            string extOnly = Path.GetExtension(fileName);
            int suffix = 1;
            do
            {
                destPath = Path.Combine(destDir, $"{nameNoExt} ({suffix}){extOnly}");
                suffix++;
            } while (pathExists(destPath));

            return destPath;
        }
    }
}
