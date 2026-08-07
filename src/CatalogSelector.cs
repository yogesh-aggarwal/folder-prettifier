using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FolderPrettifier
{
    public enum CatalogIndexStatus
    {
        NoIndex,
        NoMatch,
        Selected
    }

    public class CatalogSelection
    {
        public CatalogIndexStatus Status { get; set; }

        public string FileName { get; set; }
    }

    public static class CatalogSelector
    {
        public static Dictionary<string, string> ParseIndex(string versionsJson)
        {
            if (string.IsNullOrEmpty(versionsJson)) return null;
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(versionsJson);
            }
            catch
            {
                return null;
            }
        }

        public static CatalogSelection Select(string versionsJson, System.Version appVersion)
        {
            if (appVersion == null) throw new ArgumentNullException("appVersion");

            Dictionary<string, string> index = ParseIndex(versionsJson);
            if (index == null || index.Count == 0)
            {
                return new CatalogSelection { Status = CatalogIndexStatus.NoIndex };
            }

            System.Version best = null;
            string bestFile = null;
            foreach (KeyValuePair<string, string> entry in index)
            {
                System.Version entryVersion;
                if (System.Version.TryParse(entry.Key, out entryVersion) && entryVersion <= appVersion)
                {
                    if (best == null || entryVersion > best)
                    {
                        best = entryVersion;
                        bestFile = entry.Value;
                    }
                }
            }

            if (bestFile == null)
            {
                return new CatalogSelection { Status = CatalogIndexStatus.NoMatch };
            }

            return new CatalogSelection { Status = CatalogIndexStatus.Selected, FileName = bestFile };
        }
    }
}
