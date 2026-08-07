using System;
using System.IO;
using System.Threading.Tasks;

namespace FolderPrettifier
{
    public class CatalogLoadOutcome
    {
        public Catalog Catalog { get; set; }

        public bool UpdateRequired { get; set; }

        public string FileName { get; set; }
    }

    public class CatalogService
    {
        private readonly RemoteFileFetcher _fetcher;
        private readonly string _cacheDir;
        private readonly string _baseUrl;
        private readonly string _versionsFileName;
        private readonly Func<string> _embeddedProvider;

        public CatalogService(
            RemoteFileFetcher fetcher,
            string cacheDir,
            string baseUrl,
            string versionsFileName,
            Func<string> embeddedProvider)
        {
            _fetcher = fetcher;
            _cacheDir = cacheDir;
            _baseUrl = baseUrl;
            _versionsFileName = versionsFileName;
            _embeddedProvider = embeddedProvider;
        }

        public async Task<CatalogLoadOutcome> LoadAsync(System.Version appVersion, bool online, Action<string> onStatus = null)
        {
            string versionsCachePath = Path.Combine(_cacheDir, _versionsFileName);

            string versionsJson = null;
            if (online)
            {
                if (onStatus != null) onStatus("Checking online...");
                versionsJson = await _fetcher.FetchAsync(_baseUrl + _versionsFileName, versionsCachePath);
            }
            else
            {
                if (onStatus != null) onStatus("Offline. Using cached catalog...");
            }

            if (string.IsNullOrEmpty(versionsJson) && File.Exists(versionsCachePath))
            {
                versionsJson = File.ReadAllText(versionsCachePath);
            }

            CatalogSelection selection = CatalogSelector.Select(versionsJson, appVersion);

            string catalogJson = null;
            if (selection.Status == CatalogIndexStatus.Selected)
            {
                if (onStatus != null) onStatus("Loading catalog " + selection.FileName + "...");

                string catalogCachePath = Path.Combine(_cacheDir, selection.FileName);
                if (online)
                {
                    catalogJson = await _fetcher.FetchAsync(_baseUrl + selection.FileName, catalogCachePath);
                }
                else if (File.Exists(catalogCachePath))
                {
                    catalogJson = File.ReadAllText(catalogCachePath);
                }
            }

            if (string.IsNullOrEmpty(catalogJson))
            {
                if (onStatus != null) onStatus("Using embedded catalog...");
            }

            CatalogLoadResult result = CatalogResolver.Resolve(appVersion, versionsJson, catalogJson, _embeddedProvider());

            return new CatalogLoadOutcome
            {
                Catalog = result.Catalog,
                UpdateRequired = result.UpdateRequired,
                FileName = selection.Status == CatalogIndexStatus.Selected ? selection.FileName : null
            };
        }
    }
}
