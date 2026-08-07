using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FolderPrettifier
{
    public class UpdateInfo
    {
        public Version Version { get; set; }
        public string VersionTag { get; set; }
        public string ReleaseNotes { get; set; }
        public string AssetName { get; set; }
        public string AssetUrl { get; set; }
        public string ReleasePageUrl { get; set; }
    }

    public class UpdateService
    {
        private readonly HttpClient _httpClient;
        private readonly Func<bool> _is64BitProcess;
        private readonly string _releasesApiUrl;
        private readonly string _releasesPageUrl;

        public UpdateService(HttpMessageHandler handler = null, Func<bool> is64BitProcess = null,
            string releasesApiUrl = null, string releasesPageUrl = null)
        {
            HttpClient client = handler != null ? new HttpClient(handler) : new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FolderPrettifier");
            _httpClient = client;
            _is64BitProcess = is64BitProcess ?? (() => IntPtr.Size == 8);
            _releasesApiUrl = releasesApiUrl ?? Data.ReleasesApiUrl;
            _releasesPageUrl = releasesPageUrl ?? Data.ReleasesPageUrl;
        }

        public string ReleasePageUrl
        {
            get { return _releasesPageUrl; }
        }

        /// <summary>
        /// Checks the GitHub Releases API for the latest release and returns an UpdateInfo
        /// if it is newer than the given version. Returns null when there is no update,
        /// when the release cannot be parsed, or when the request fails.
        /// </summary>
        public async Task<UpdateInfo> CheckForUpdateAsync(Version currentVersion)
        {
            try
            {
                string json;
                using (HttpResponseMessage response = await _httpClient.GetAsync(_releasesApiUrl))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }
                    json = await response.Content.ReadAsStringAsync();
                }
                return ParseReleaseJson(json, currentVersion);
            }
            catch
            {
                return null;
            }
        }

        public UpdateInfo ParseReleaseJson(string json, Version currentVersion)
        {
            if (string.IsNullOrWhiteSpace(json) || currentVersion == null)
            {
                return null;
            }

            JObject release;
            try
            {
                release = JObject.Parse(json);
            }
            catch
            {
                return null;
            }

            string tag = release["tag_name"] != null ? release["tag_name"].ToString() : "";
            Version latest = ParseTagVersion(tag);
            if (latest == null || latest <= currentVersion)
            {
                return null;
            }

            string assetName = null;
            string assetUrl = null;
            string arch = _is64BitProcess() ? "x64" : "x86";
            string prefix = "FolderPrettifier-Portable-" + arch + "-";
            string legacyToken = _is64BitProcess() ? "64-bit" : "32-bit";
            JArray assets = release["assets"] as JArray;
            if (assets != null)
            {
                foreach (JToken asset in assets)
                {
                    string name = asset["name"] != null ? asset["name"].ToString() : "";
                    bool matches = name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
                    if (!matches)
                    {
                        matches = name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                            && (name.IndexOf(legacyToken, StringComparison.OrdinalIgnoreCase) >= 0);
                    }
                    if (matches)
                    {
                        assetName = name;
                        assetUrl = asset["browser_download_url"] != null ? asset["browser_download_url"].ToString() : null;
                        break;
                    }
                }
            }

            string htmlUrl = release["html_url"] != null ? release["html_url"].ToString() : _releasesPageUrl;
            if (string.IsNullOrEmpty(htmlUrl))
            {
                htmlUrl = _releasesPageUrl;
            }

            return new UpdateInfo
            {
                Version = latest,
                VersionTag = tag,
                ReleaseNotes = release["body"] != null ? release["body"].ToString() : "",
                AssetName = assetName,
                AssetUrl = assetUrl,
                ReleasePageUrl = htmlUrl
            };
        }

        private static Version ParseTagVersion(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return null;
            }

            string candidate = tag.Trim();
            if (candidate.Length > 0 && (candidate[0] == 'v' || candidate[0] == 'V'))
            {
                candidate = candidate.Substring(1);
            }

            Version version;
            if (!Version.TryParse(candidate, out version))
            {
                return null;
            }

            return new Version(version.Major, version.Minor, Math.Max(0, version.Build), Math.Max(0, version.Revision));
        }

        /// <summary>
        /// Downloads the update asset to the given path, reporting download progress (0-100).
        /// Returns true on success. The download is skipped (and true returned) if the file
        /// already exists at the destination.
        /// </summary>
        public async Task<bool> DownloadAsync(UpdateInfo update, string destinationPath, IProgress<int> progress)
        {
            if (update == null || string.IsNullOrEmpty(update.AssetUrl) || string.IsNullOrEmpty(destinationPath))
            {
                return false;
            }

            if (File.Exists(destinationPath))
            {
                if (progress != null)
                {
                    progress.Report(100);
                }
                return true;
            }

            try
            {
                using (HttpResponseMessage response = await _httpClient.GetAsync(update.AssetUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    long total = response.Content.Headers.ContentLength ?? -1;
                    using (Stream input = await response.Content.ReadAsStreamAsync())
                    using (FileStream output = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[81920];
                        long received = 0;
                        int read;
                        while ((read = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, read);
                            received += read;
                            if (progress != null && total > 0)
                            {
                                progress.Report((int)(received * 100 / total));
                            }
                        }
                    }
                }

                if (progress != null)
                {
                    progress.Report(100);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the full path of the running executable.
        /// </summary>
        public static string GetExecutablePath()
        {
            try
            {
                return Process.GetCurrentProcess().MainModule.FileName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Returns true when the running executable can be replaced in place
        /// (i.e. its folder is writable). When false, an update must be installed
        /// manually from the release page.
        /// </summary>
        public static bool CanUpdateInPlace()
        {
            string exePath = GetExecutablePath();
            if (string.IsNullOrEmpty(exePath))
            {
                return false;
            }

            string dir = Path.GetDirectoryName(exePath);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                return false;
            }

            try
            {
                string probe = Path.Combine(dir, ".fpf-write-probe-" + Guid.NewGuid().ToString("N"));
                File.WriteAllText(probe, "probe");
                File.Delete(probe);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Replaces the running executable with the downloaded update via a helper
        /// batch script: the script waits for this process to exit, copies the new
        /// executable over the old one, and relaunches the app. Returns true when the
        /// updater was launched; the caller should then exit the application.
        /// </summary>
        public bool ApplyUpdate(string downloadedFilePath)
        {
            if (string.IsNullOrEmpty(downloadedFilePath) || !File.Exists(downloadedFilePath) || !CanUpdateInPlace())
            {
                return false;
            }

            try
            {
                string targetExe = GetExecutablePath();
                int pid = Process.GetCurrentProcess().Id;
                string scriptDir = Path.Combine(Path.GetTempPath(), "FolderPrettifier-Updater-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(scriptDir);
                string scriptPath = Path.Combine(scriptDir, "apply-update.bat");

                string script =
                    "@echo off\r\n" +
                    "setlocal\r\n" +
                    "set tries=0\r\n" +
                    ":wait\r\n" +
                    "tasklist /FI \"PID eq " + pid + "\" | findstr \"" + pid + "\" >nul\r\n" +
                    "if not errorlevel 1 (\r\n" +
                    "  ping 127.0.0.1 -n 2 >nul\r\n" +
                    "  goto wait\r\n" +
                    ")\r\n" +
                    ":copy\r\n" +
                    "set /a tries+=1\r\n" +
                    "copy /y \"" + downloadedFilePath + "\" \"" + targetExe + "\" >nul\r\n" +
                    "if not errorlevel 1 (\r\n" +
                    "  start \"\" \"" + targetExe + "\"\r\n" +
                    "  del \"%~f0\" >nul 2>nul\r\n" +
                    "  rmdir /s /q \"" + scriptDir + "\" >nul 2>nul\r\n" +
                    "  exit /b 0\r\n" +
                    ")\r\n" +
                    "if %tries% lss 30 (\r\n" +
                    "  ping 127.0.0.1 -n 2 >nul\r\n" +
                    "  goto copy\r\n" +
                    ")\r\n" +
                    "del \"%~f0\" >nul 2>nul\r\n" +
                    "rmdir /s /q \"" + scriptDir + "\" >nul 2>nul\r\n" +
                    "exit /b 1\r\n";
                File.WriteAllText(scriptPath, script);

                ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", "/c \"\"" + scriptPath + "\"\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Path.GetTempPath()
                };
                Process.Start(startInfo);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
