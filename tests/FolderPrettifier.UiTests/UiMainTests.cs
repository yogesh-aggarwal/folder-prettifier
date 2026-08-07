using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA3;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FolderPrettifier.UiTests
{
    [TestFixture]
    [NonParallelizable]
    public class UiMainTests
    {
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

        private const uint WM_COMMAND = 0x0111;
        private const uint WM_SETTEXT = 0x000C;
        private const uint WM_GETTEXT = 0x000D;
        private const uint WM_CLOSE = 0x0010;
        private const uint BM_CLICK = 0x00F5;

        private static readonly Dictionary<string, IntPtr> MessageBoxButtonIds = new Dictionary<string, IntPtr>
        {
            { "OK", (IntPtr)1 },
            { "Cancel", (IntPtr)2 },
            { "Abort", (IntPtr)3 },
            { "Retry", (IntPtr)4 },
            { "Ignore", (IntPtr)5 },
            { "Yes", (IntPtr)6 },
            { "No", (IntPtr)7 }
        };

        private static string FindExe()
        {
            string env = Environment.GetEnvironmentVariable("FP_EXE_PATH");
            if (!string.IsNullOrEmpty(env) && File.Exists(env)) return env;

            DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "src", "Build", "Debug", "x86", "Folder Prettifier.exe");
                if (File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            throw new InvalidOperationException("Folder Prettifier.exe not found. Set FP_EXE_PATH or build Debug AnyCPU first.");
        }

        private static void WaitUntil(Func<bool> condition, int timeoutMs, string what)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (condition()) return;
                Thread.Sleep(200);
            }
            throw new TimeoutException("Timed out waiting for: " + what);
        }

        private string _stageDir;
        private string _tempDir;
        private int _processId;
        private UIA3Automation _automation;
        private Window _mainWindow;

        private void LaunchApp()
        {
            _stageDir = Path.Combine(Path.GetTempPath(), "fp-ui-" + Guid.NewGuid().ToString("N"));
            _tempDir = Path.Combine(_stageDir, "Source");
            Directory.CreateDirectory(_tempDir);

            Process process = Process.Start(new ProcessStartInfo(FindExe(), "\"" + _tempDir + "\""));
            _processId = process.Id;
            _automation = new UIA3Automation();

            IntPtr hwnd = IntPtr.Zero;
            WaitUntil(() =>
            {
                hwnd = FindWindowByTitle("Folder Prettifier");
                return hwnd != IntPtr.Zero;
            }, 30000, "main window");
            _mainWindow = _automation.FromHandle(hwnd).AsWindow();
        }

        [TearDown]
        public void TearDown()
        {
            if (_automation != null)
            {
                try { _automation.Dispose(); } catch { }
            }
            if (_processId != 0)
            {
                try
                {
                    Process p = Process.GetProcessById(_processId);
                    if (!p.HasExited) p.Kill();
                }
                catch { }
            }
            if (_stageDir != null && Directory.Exists(_stageDir))
            {
                try { Directory.Delete(_stageDir, true); } catch { }
            }
        }

        private IntPtr FindWindowByPid(int pid)
        {
            IntPtr found = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                uint windowPid;
                GetWindowThreadProcessId(hWnd, out windowPid);
                if (windowPid == pid)
                {
                    found = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }

        private IntPtr FindWindowByTitle(string title)
        {
            IntPtr found = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                uint pid;
                GetWindowThreadProcessId(hWnd, out pid);
                if (pid == _processId)
                {
                    StringBuilder sb = new StringBuilder(256);
                    GetWindowText(hWnd, sb, 256);
                    if (sb.ToString() == title)
                    {
                        found = hWnd;
                        return false;
                    }
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }

        private void WaitUntilStartEnabled()
        {
            Exception lastError = null;
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 30000)
            {
                try
                {
                    var btn = _mainWindow.FindFirstDescendant(cf => cf.ByName("Start").And(cf.ByControlType(ControlType.Button)));
                    if (btn != null && btn.Properties.IsEnabled.Value) return;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
                Thread.Sleep(200);
            }

            string dump = "";
            try
            {
                foreach (var e in _mainWindow.FindAllDescendants())
                {
                    dump += string.Format("\n  name='{0}' type={1} enabled={2}",
                        e.Properties.Name.Value, e.Properties.ControlType.Value, e.Properties.IsEnabled.Value);
                }
            }
            catch (Exception ex)
            {
                dump = "dump failed: " + ex.Message;
            }
            throw new TimeoutException("Start button never enabled."
                + (lastError != null ? " last error: " + lastError.Message : "")
                + dump);
        }

        private void ToggleCheckBox(string name, bool on)
        {
            var cb = _mainWindow.FindFirstDescendant(cf => cf.ByName(name).And(cf.ByControlType(ControlType.CheckBox)));
            Assert.That(cb, Is.Not.Null, "Checkbox not found: " + name);
            if (cb.AsCheckBox().IsChecked != on)
            {
                cb.AsCheckBox().Toggle();
            }
        }

        private void ClickMessageBox(string title, string buttonText, int timeoutMs)
        {
            IntPtr dialog = IntPtr.Zero;
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                dialog = FindWindowByTitle(title);
                if (dialog != IntPtr.Zero) break;
                Thread.Sleep(200);
            }
            if (dialog == IntPtr.Zero) throw new TimeoutException("Dialog not found: " + title);

            if (buttonText == "OK")
            {
                // The app ignores the result of OK-only dialogs, so closing them is enough.
                // (WM_COMMAND IDOK does not reliably dismiss them on this system.)
                SendMessage(dialog, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                if (WaitForDialogGone(title, 3000)) return;
                SendMessage(dialog, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                WaitUntil(() => FindWindowByTitle(title) == IntPtr.Zero, 3000, "dialog closed: " + title);
                return;
            }

            IntPtr buttonId;
            if (!MessageBoxButtonIds.TryGetValue(buttonText, out buttonId))
            {
                throw new InvalidOperationException("Unknown message box button: " + buttonText);
            }

            // Send the command and wait for the dialog to actually close; if the
            // command raced ahead of the modal loop, send it again until it sticks.
            for (int attempt = 0; attempt < 10; attempt++)
            {
                SendMessage(dialog, WM_COMMAND, buttonId, IntPtr.Zero);
                if (WaitForDialogGone(title, 1500)) return;
                Thread.Sleep(200);
            }
            throw new TimeoutException("Dialog did not close: " + title);
        }

        private bool WaitForDialogGone(string title, int timeoutMs)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (FindWindowByTitle(title) == IntPtr.Zero) return true;
                Thread.Sleep(100);
            }
            return false;
        }

        private void ClickStartAndHandleDialogs()
        {
            var startBtn = _mainWindow.FindFirstDescendant(cf => cf.ByName("Start").And(cf.ByControlType(ControlType.Button)));
            Assert.That(startBtn, Is.Not.Null, "Start button not found");
            IntPtr startHwnd = (IntPtr)startBtn.Properties.NativeWindowHandle.Value;

            // BM_CLICK is synchronous: it blocks until the click handler returns, and the
            // handler blocks at the modal ATTENTION dialog. Click from a worker thread and
            // dismiss the dialogs from this thread.
            Task click = Task.Run(() => SendMessage(startHwnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero));
            ClickMessageBox("ATTENTION!", "Yes", 15000);
            ClickMessageBox("Enjoy!", "OK", 30000);
            click.Wait(20000);
        }

        private void CreateFile(string relativePath)
        {
            string full = Path.Combine(_tempDir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, "content");
        }

        private static string DumpTree(string root)
        {
            StringBuilder sb = new StringBuilder();
            if (!Directory.Exists(root))
            {
                return root + " does not exist";
            }
            foreach (string dir in Directory.GetDirectories(root, "*", SearchOption.AllDirectories))
            {
                sb.AppendLine(dir);
            }
            foreach (string file in Directory.GetFiles(root, "*", SearchOption.AllDirectories))
            {
                sb.AppendLine(file);
            }
            return sb.ToString();
        }

        [Test]
        public void PrettifyAndCategorize_EndToEnd_MovesAndRenamesFiles()
        {
            LaunchApp();
            CreateFile("hello.txt");
            CreateFile("movie.mp4");
            CreateFile("doc.pages");

            WaitUntilStartEnabled();
            ToggleCheckBox("Prettify Name", true);
            ToggleCheckBox("Capitalize", true);
            ToggleCheckBox("Categorize Files", true);

            ClickStartAndHandleDialogs();

            try
            {
                WaitUntil(() =>
                    File.Exists(Path.Combine(_tempDir, "Documents", "Text", "Hello.txt")) &&
                    File.Exists(Path.Combine(_tempDir, "Videos", "Movie.mp4")) &&
                    File.Exists(Path.Combine(_tempDir, "Documents", "Office", "Doc.pages")), 15000, "files processed");
            }
            catch (TimeoutException)
            {
                Assert.Fail("Files not processed. Folder state:\n" + DumpTree(_tempDir));
            }

            Assert.That(File.Exists(Path.Combine(_tempDir, "hello.txt")), Is.False);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Documents", "Text", "Hello.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Videos", "Movie.mp4")), Is.True);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Documents", "Office", "Doc.pages")), Is.True);
        }

        [Test]
        public void RenameFolder_EndToEnd_RenamesTargetFolder()
        {
            LaunchApp();
            CreateFile("a.txt");

            WaitUntilStartEnabled();
            ToggleCheckBox("Categorize Files", true);

            var renameTo = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("renameTo"));
            Assert.That(renameTo, Is.Not.Null, "renameTo textbox not found");
            renameTo.AsTextBox().Text = "Renamed";

            ClickStartAndHandleDialogs();

            string renamed = Path.Combine(_stageDir, "Renamed");
            WaitUntil(() => Directory.Exists(renamed), 15000, "folder renamed");

            Assert.That(Directory.Exists(_tempDir), Is.False);
            Assert.That(Directory.Exists(renamed), Is.True);
        }

        [Test]
        public void Sanitization_InvalidCharactersRemovedFromPrefix()
        {
            LaunchApp();
            CreateFile("a.txt");

            WaitUntilStartEnabled();
            ToggleCheckBox("Prettify Name", true);
            ToggleCheckBox("Name With", true);

            var startsWith = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("nameStartsWith"));
            Assert.That(startsWith, Is.Not.Null, "nameStartsWith textbox not found");
            IntPtr tbHwnd = (IntPtr)startsWith.Properties.NativeWindowHandle.Value;

            // WM_SETTEXT is synchronous and triggers the sanitization dialog on the
            // app thread; set it from a worker thread and dismiss the dialog here.
            Task setText = Task.Run(() => SendMessage(tbHwnd, WM_SETTEXT, IntPtr.Zero, "a<b"));
            ClickMessageBox("Invalid name", "OK", 15000);
            setText.Wait(10000);

            WaitUntil(() =>
            {
                StringBuilder sb = new StringBuilder(256);
                SendMessage(tbHwnd, WM_GETTEXT, (IntPtr)256, sb);
                return sb.ToString() == "ab";
            }, 10000, "sanitized prefix text");
            StringBuilder actual = new StringBuilder(256);
            SendMessage(tbHwnd, WM_GETTEXT, (IntPtr)256, actual);
            Assert.That(actual.ToString(), Is.EqualTo("ab"), "actual textbox text after sanitize: '" + actual + "'" + " setTextFaulted=" + setText.IsFaulted);
        }

        [Test]
        public void UpdateCatalogMenu_RefreshesAndReturnsToReady()
        {
            LaunchApp();

            WaitUntilStartEnabled();

            var updateItem = _mainWindow.FindFirstDescendant(cf => cf.ByName("Update Catalog"));
            Assert.That(updateItem, Is.Not.Null);
            try
            {
                updateItem.Patterns.Invoke.Pattern.Invoke();
            }
            catch
            {
                updateItem.Focus();
                Keyboard.Type(VirtualKeyShort.RETURN);
            }

            WaitUntil(() =>
                _mainWindow.FindFirstDescendant(cf => cf.ByName("Fetching Catalog...")) != null, 15000, "catalog refresh started");
            WaitUntil(() =>
                _mainWindow.FindFirstDescendant(cf => cf.ByName("Ready")) != null, 30000, "catalog refresh finished");

            var startBtn = _mainWindow.FindFirstDescendant(cf => cf.ByName("Start").And(cf.ByControlType(ControlType.Button)));
            Assert.That(startBtn.Properties.IsEnabled.Value, Is.True);
        }
    }
}
