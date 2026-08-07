using System.IO;

namespace FolderPrettifier
{
    public static class LegacyCacheCleaner
    {
        public static void Clean(string directory)
        {
            if (string.IsNullOrEmpty(directory)) return;

            try
            {
                string legacyFile = Path.Combine(directory, "cat.fpc");
                if (File.Exists(legacyFile))
                {
                    File.Delete(legacyFile);
                }
            }
            catch
            {
            }
        }
    }
}
