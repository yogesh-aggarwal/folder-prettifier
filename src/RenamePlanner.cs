using System.IO;

namespace FolderPrettifier
{
    public class RenamePlan
    {
        public string TargetPath { get; set; }

        public bool IsRename { get; set; }

        public bool Conflict { get; set; }
    }

    public static class RenamePlanner
    {
        public static RenamePlan Plan(string srcFolder, string renameTarget)
        {
            if (string.IsNullOrEmpty(renameTarget))
            {
                return new RenamePlan();
            }

            string parentDir = Path.GetDirectoryName(srcFolder);
            string newName = Path.Combine(parentDir, renameTarget);

            bool isRename = srcFolder != newName;

            return new RenamePlan
            {
                TargetPath = newName,
                IsRename = isRename,
                Conflict = isRename && Directory.Exists(newName)
            };
        }
    }
}
