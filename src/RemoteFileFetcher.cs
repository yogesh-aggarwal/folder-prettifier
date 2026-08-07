using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FolderPrettifier
{
    public class RemoteFileFetcher
    {
        private readonly HttpClient _httpClient;

        public RemoteFileFetcher(HttpMessageHandler handler = null)
        {
            _httpClient = handler != null ? new HttpClient(handler) : new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FolderPrettifier");
        }

        public async Task<string> FetchAsync(string url, string cachePath)
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (File.Exists(cachePath))
                {
                    request.Headers.IfModifiedSince = File.GetLastWriteTimeUtc(cachePath);
                }

                try
                {
                    using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                    {
                        if (response.StatusCode == HttpStatusCode.NotModified)
                        {
                            return File.ReadAllText(cachePath);
                        }

                        if (!response.IsSuccessStatusCode)
                        {
                            return null;
                        }

                        string content = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(content))
                        {
                            return null;
                        }

                        string dir = Path.GetDirectoryName(cachePath);
                        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        File.WriteAllText(cachePath, content);
                        return content;
                    }
                }
                catch
                {
                    return null;
                }
            }
        }

        public async Task<bool> CheckAsync(string url, TimeSpan timeout)
        {
            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(timeout))
                using (HttpResponseMessage response = await _httpClient.GetAsync(url, cts.Token))
                {
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
