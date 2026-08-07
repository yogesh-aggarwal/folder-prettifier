using System;
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

            // A folder name can never contain separators. Rejecting them here
            // also blocks "." / ".." / "..\.." escapes through Path.Combine.
            if (renameTarget.IndexOf('\\') >= 0 || renameTarget.IndexOf('/') >= 0)
            {
                return new RenamePlan();
            }

            // Windows strips trailing dots and spaces from a name, so a target
            // that is only dots/whitespace (".", "..", " ", "...") is not a
            // real rename. Normalize to what the filesystem would store.
            string targetName = renameTarget.TrimEnd(' ', '.');
            if (targetName.Length == 0)
            {
                return new RenamePlan();
            }

            string parentDir = Path.GetDirectoryName(srcFolder);
            string newName = Path.Combine(parentDir, targetName);

            // Compare with the filesystem's semantics. "Source" -> "source" is
            // a legitimate case-only rename of the same folder: it must never
            // be treated as a conflict against (and deletion of) itself.
            bool sameFolderAsSource = string.Equals(srcFolder, newName, StringComparison.OrdinalIgnoreCase);
            bool isRename = !string.Equals(srcFolder, newName, StringComparison.Ordinal);

            return new RenamePlan
            {
                TargetPath = newName,
                IsRename = isRename,
                Conflict = isRename && !sameFolderAsSource && Directory.Exists(newName)
            };
        }
    }
}
