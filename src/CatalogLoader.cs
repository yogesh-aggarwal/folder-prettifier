using System;
using System.Threading.Tasks;

namespace FolderPrettifier
{
    public interface ICatalogUi
    {
        void SetStatus(string text);
        void SetProgress(int value);
        void SetStartEnabled(bool enabled);
        void SetCatalogRefreshEnabled(bool enabled);
        void ShowUpdateRequired(Version appVersion);
    }

    public class CatalogLoader
    {
        private readonly RemoteFileFetcher _fetcher;
        private readonly CatalogService _catalogService;
        private readonly Func<Version> _getAppVersion;
        private readonly ICatalogUi _ui;
        private readonly string _internetCheckUrl;
        private readonly TimeSpan _internetCheckTimeout;
        private readonly int _readyDelayMs;

        public CatalogLoader(RemoteFileFetcher fetcher, CatalogService catalogService,
            Func<Version> getAppVersion, ICatalogUi ui,
            string internetCheckUrl = null, TimeSpan? internetCheckTimeout = null, int readyDelayMs = 500)
        {
            _fetcher = fetcher;
            _catalogService = catalogService;
            _getAppVersion = getAppVersion;
            _ui = ui;
            _internetCheckUrl = internetCheckUrl ?? Data.InternetCheckUrl;
            _internetCheckTimeout = internetCheckTimeout ?? TimeSpan.FromSeconds(3);
            _readyDelayMs = readyDelayMs;
        }

        public async Task<CatalogLoadResult> LoadAsync()
        {
            _ui.SetStatus("Fetching Catalog...");
            _ui.SetStartEnabled(false);
            _ui.SetCatalogRefreshEnabled(false);
            _ui.SetProgress(0);

            Version appVersion = _getAppVersion();

            bool online = await _fetcher.CheckAsync(_internetCheckUrl, _internetCheckTimeout);
            _ui.SetProgress(30);

            CatalogLoadOutcome outcome = await _catalogService.LoadAsync(appVersion, online, s => _ui.SetStatus(s));

            if (outcome.UpdateRequired)
            {
                _ui.ShowUpdateRequired(appVersion);
                return new CatalogLoadResult { UpdateRequired = true };
            }

            if (outcome.Catalog == null)
            {
                _ui.SetStatus("No catalog! Can't proceed");
                _ui.SetCatalogRefreshEnabled(true);
                return new CatalogLoadResult { UpdateRequired = false };
            }

            _ui.SetCatalogRefreshEnabled(true);
            _ui.SetProgress(100);

            await Task.Delay(_readyDelayMs);
            _ui.SetStatus("Ready");
            _ui.SetProgress(0);

            return new CatalogLoadResult { Catalog = outcome.Catalog, UpdateRequired = false };
        }
    }
}
