using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace FolderPrettifier
{
    public class CatalogCategory
    {
        public string Name { get; set; }

        public string Folder { get; set; }

        public List<string> Extensions { get; set; }
    }

    public class Catalog
    {
        public int Version { get; set; }

        [JsonProperty("min-app-version")]
        public string MinAppVersion { get; set; }

        public string Default { get; set; }

        public List<CatalogCategory> Categories { get; set; }

        public string DefaultFolder
        {
            get { return NormalizeFolder(string.IsNullOrEmpty(Default) ? "Others/Unknown" : Default); }
        }

        public bool IsCompatibleWith(System.Version appVersion)
        {
            System.Version minVersion;
            return System.Version.TryParse(MinAppVersion, out minVersion) && appVersion >= minVersion;
        }

        public Dictionary<string, string> BuildExtensionMap()
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (Categories != null)
            {
                foreach (CatalogCategory category in Categories)
                {
                    if (category == null || category.Extensions == null) continue;

                    string folder = NormalizeFolder(category.Folder);
                    if (string.IsNullOrEmpty(folder)) continue;

                    foreach (string ext in category.Extensions)
                    {
                        if (!string.IsNullOrEmpty(ext))
                        {
                            map[ext.TrimStart('.').ToLowerInvariant()] = folder;
                        }
                    }
                }
            }
            return map;
        }

        private static string NormalizeFolder(string folder)
        {
            return string.IsNullOrEmpty(folder) ? folder : folder.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
