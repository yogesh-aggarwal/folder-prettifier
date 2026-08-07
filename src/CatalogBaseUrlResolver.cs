using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FolderPrettifier
{
    public class CatalogBaseUrlResolver
    {
        private readonly RemoteFileFetcher _fetcher;
        private readonly string _repoApiUrl;
        private readonly string _rawBaseUrlTemplate;
        private readonly string _repoInfoCachePath;

        public CatalogBaseUrlResolver(
            RemoteFileFetcher fetcher,
            string repoApiUrl,
            string rawBaseUrlTemplate,
            string repoInfoCachePath)
        {
            _fetcher = fetcher;
            _repoApiUrl = repoApiUrl;
            _rawBaseUrlTemplate = rawBaseUrlTemplate;
            _repoInfoCachePath = repoInfoCachePath;
        }

        public async Task<string> ResolveBaseUrlAsync()
        {
            string repoInfo = await _fetcher.FetchAsync(_repoApiUrl, _repoInfoCachePath);
            if (string.IsNullOrEmpty(repoInfo) && File.Exists(_repoInfoCachePath))
            {
                repoInfo = File.ReadAllText(_repoInfoCachePath);
            }

            string defaultBranch = ExtractDefaultBranch(repoInfo);
            if (string.IsNullOrEmpty(defaultBranch))
            {
                return null;
            }

            return string.Format(_rawBaseUrlTemplate, defaultBranch);
        }

        private static string ExtractDefaultBranch(string repoInfo)
        {
            if (string.IsNullOrEmpty(repoInfo)) return null;

            try
            {
                JObject json = JObject.Parse(repoInfo);
                string branch = (string)json["default_branch"];
                return string.IsNullOrEmpty(branch) ? null : branch;
            }
            catch
            {
                return null;
            }
        }
    }
}
