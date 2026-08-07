using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
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
                string dir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

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

                        if (total > 0 && received != total)
                        {
                            throw new IOException("Download size mismatch: expected " + total + " bytes, received " + received);
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
                TryDeleteFile(destinationPath);
                return false;
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
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

            return CanUpdateInPlaceIn(Path.GetDirectoryName(exePath));
        }

        /// <summary>
        /// Returns true when files can be created and deleted inside the given directory
        /// (i.e. the running executable can be replaced in place there).
        /// </summary>
        public static bool CanUpdateInPlaceIn(string dir)
        {
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
        /// Launches the downloaded update executable in updater mode so it can replace
        /// this running executable: the updater waits for this process to exit, copies
        /// itself over this executable, and relaunches the app. Returns true when the
        /// updater was launched; the caller should then exit the application.
        ///
        /// The updater mode is invoked by passing --apply-update to the new executable,
        /// which avoids helper scripts and shell quoting entirely: process arguments
        /// are passed verbatim, so paths containing spaces or any other characters are
        /// handled safely.
        /// </summary>
        public virtual bool LaunchUpdater(string downloadedFilePath)
        {
            if (string.IsNullOrEmpty(downloadedFilePath) || !File.Exists(downloadedFilePath) || !CanUpdateInPlace())
            {
                return false;
            }

            try
            {
                string targetExe = GetExecutablePath();
                int pid = Process.GetCurrentProcess().Id;

                ProcessStartInfo startInfo = new ProcessStartInfo(downloadedFilePath, BuildUpdaterArguments(targetExe, pid))
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process.Start(startInfo);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Builds the command-line arguments that put the new executable into updater
        /// mode: --apply-update &lt;target executable&gt; &lt;current process id&gt;.
        /// </summary>
        public static string BuildUpdaterArguments(string targetExe, int currentPid)
        {
            return "--apply-update \"" + targetExe + "\" " + currentPid;
        }

        /// <summary>
        /// Parses the updater-mode command line arguments, as produced by
        /// BuildUpdaterArguments: --apply-update &lt;target executable&gt; &lt;old process id&gt;.
        /// Returns true and fills the out parameters when the arguments are valid.
        /// </summary>
        public static bool TryParseUpdaterArgs(string[] args, out string targetExe, out int oldPid)
        {
            targetExe = null;
            oldPid = 0;

            if (args == null || args.Length < 3 || args[0] != "--apply-update")
            {
                return false;
            }

            string exe = args[1];
            if (string.IsNullOrWhiteSpace(exe))
            {
                return false;
            }

            if (!int.TryParse(args[2], out oldPid))
            {
                return false;
            }

            targetExe = exe;
            return true;
        }

        /// <summary>
        /// Runs in updater mode (invoked by the new executable with --apply-update):
        /// waits for the old process to exit, replaces the old executable with this
        /// process's own file, then relaunches the app.
        /// </summary>
        public static void RunUpdater(string targetExe, int oldPid)
        {
            RunUpdater(Assembly.GetExecutingAssembly().Location, targetExe, oldPid, 30, 500);
        }

        /// <summary>
        /// Core updater routine, exposed for testing.
        /// </summary>
        public static void RunUpdater(string updaterExe, string targetExe, int oldPid, int retryCount, int retryDelayMs)
        {
            try
            {
                if (string.IsNullOrEmpty(updaterExe) || !File.Exists(updaterExe) || string.IsNullOrEmpty(targetExe))
                {
                    return;
                }

                Process oldProcess = null;
                try
                {
                    oldProcess = Process.GetProcessById(oldPid);
                }
                catch (ArgumentException)
                {
                }
                catch (InvalidOperationException)
                {
                }

                if (oldProcess != null)
                {
                    oldProcess.WaitForExit(120000);
                }

                for (int attempt = 0; attempt < retryCount; attempt++)
                {
                    try
                    {
                        File.Copy(updaterExe, targetExe, true);
                        break;
                    }
                    catch
                    {
                        if (attempt == retryCount - 1)
                        {
                            return;
                        }
                        Thread.Sleep(retryDelayMs);
                    }
                }

                Process.Start(new ProcessStartInfo(targetExe) { UseShellExecute = true });
            }
            catch
            {
            }
        }
    }
}
