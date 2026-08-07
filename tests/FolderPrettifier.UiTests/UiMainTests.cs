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

        private void LaunchApp(params string[] filesToCreate)
        {
            LaunchAppInternal(true, filesToCreate);
        }

        private void LaunchAppWithoutFolderArg()
        {
            LaunchAppInternal(false, new string[0]);
        }

        private void LaunchAppInternal(bool passFolderArg, string[] filesToCreate)
        {
            _stageDir = Path.Combine(Path.GetTempPath(), "fp-ui-" + Guid.NewGuid().ToString("N"));
            _tempDir = Path.Combine(_stageDir, "Source");
            Directory.CreateDirectory(_tempDir);
            foreach (string f in filesToCreate)
            {
                CreateFile(f);
            }

            string folderArg = passFolderArg ? "\"" + _tempDir + "\"" : "";
            Process process = Process.Start(new ProcessStartInfo(FindExe(), folderArg));
            _processId = process.Id;
            _automation = new UIA3Automation();

            // Find the window and build the automation element. UIA can be slow to
            // attach right after process start on CI runners, so retry instead of
            // failing on the first ElementFromHandle timeout.
            WaitUntil(() =>
            {
                IntPtr hwnd = FindWindowByTitle("Folder Prettifier");
                if (hwnd == IntPtr.Zero)
                {
                    return false;
                }
                try
                {
                    _mainWindow = _automation.FromHandle(hwnd).AsWindow();
                    return true;
                }
                catch
                {
                    return false;
                }
            }, 30000, "main window automation element");
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

        private IntPtr FindAnyWindowByTitlePrefix(string titlePrefix)
        {
            IntPtr found = IntPtr.Zero;
            EnumWindows((hWnd, lParam) =>
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, 256);
                string title = sb.ToString();
                if (title == titlePrefix || title.StartsWith(titlePrefix + " - File Explorer"))
                {
                    found = hWnd;
                    return false;
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

        private bool IsControlEnabled(string automationId)
        {
            var el = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            return el != null && el.Properties.IsEnabled.Value;
        }

        private string LabelText(string automationId)
        {
            var el = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            return el == null ? null : el.Properties.Name.Value;
        }

        private string GetTextBoxText(string automationId)
        {
            var el = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            Assert.That(el, Is.Not.Null, "Textbox not found: " + automationId);
            IntPtr hwnd = (IntPtr)el.Properties.NativeWindowHandle.Value;
            StringBuilder sb = new StringBuilder(256);
            SendMessage(hwnd, WM_GETTEXT, (IntPtr)256, sb);
            return sb.ToString();
        }

        private void AssertEnabledState(string automationId, bool expected)
        {
            WaitUntil(() =>
            {
                try
                {
                    return IsControlEnabled(automationId) == expected;
                }
                catch
                {
                    return false;
                }
            }, 5000, "control " + automationId + (expected ? " enabled" : " disabled"));
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
        public void CheckboxMatrix_SubOptionsFollowPrettifyToggle()
        {
            LaunchApp();
            WaitUntilStartEnabled();

            AssertEnabledState("isCapitalizeName", false);
            AssertEnabledState("isReplaceWord", false);
            AssertEnabledState("isNameWith", false);
            AssertEnabledState("replaceWord", false);
            AssertEnabledState("withWord", false);
            AssertEnabledState("nameStartsWith", false);
            AssertEnabledState("nameEndsWith", false);

            ToggleCheckBox("Prettify Name", true);
            AssertEnabledState("isCapitalizeName", true);
            AssertEnabledState("isReplaceWord", true);
            AssertEnabledState("isNameWith", true);
            AssertEnabledState("replaceWord", false);
            AssertEnabledState("withWord", false);
            AssertEnabledState("nameStartsWith", false);
            AssertEnabledState("nameEndsWith", false);

            ToggleCheckBox("Replace Specific Word", true);
            AssertEnabledState("replaceWord", true);
            AssertEnabledState("withWord", true);
            AssertEnabledState("nameStartsWith", false);
            AssertEnabledState("nameEndsWith", false);

            ToggleCheckBox("Name With", true);
            AssertEnabledState("nameStartsWith", true);
            AssertEnabledState("nameEndsWith", true);

            ToggleCheckBox("Prettify Name", false);
            AssertEnabledState("isCapitalizeName", false);
            AssertEnabledState("isReplaceWord", false);
            AssertEnabledState("isNameWith", false);
            AssertEnabledState("replaceWord", false);
            AssertEnabledState("withWord", false);
            AssertEnabledState("nameStartsWith", false);
            AssertEnabledState("nameEndsWith", false);
        }

        [Test]
        public void LocationCount_UpdatesFileCountAndRenameTarget()
        {
            LaunchApp("a.txt", "b.txt", "c.txt");
            string other = Path.Combine(_stageDir, "Other");
            Directory.CreateDirectory(other);
            File.WriteAllText(Path.Combine(other, "x.txt"), "x");
            File.WriteAllText(Path.Combine(other, "y.txt"), "y");

            WaitUntilStartEnabled();
            WaitUntil(() => LabelText("totalFilesCount") == "3", 10000, "file count shows 3");
            Assert.That(GetTextBoxText("renameTo"), Is.EqualTo("Source"));

            var location = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("location"));
            Assert.That(location, Is.Not.Null, "location textbox not found");
            IntPtr locationHwnd = (IntPtr)location.Properties.NativeWindowHandle.Value;
            SendMessage(locationHwnd, WM_SETTEXT, IntPtr.Zero, other);

            WaitUntil(() => LabelText("totalFilesCount") == "2", 10000, "file count shows 2");
            Assert.That(GetTextBoxText("renameTo"), Is.EqualTo("Other"));
        }

        [Test]
        public void InaccessibleFolder_ShowsErrorAndSkipsEnjoy()
        {
            LaunchApp("a.txt");
            WaitUntilStartEnabled();

            // Delete the folder behind the app's back, then start the run
            Directory.Delete(_tempDir, true);

            var startBtn = _mainWindow.FindFirstDescendant(cf => cf.ByName("Start").And(cf.ByControlType(ControlType.Button)));
            Assert.That(startBtn, Is.Not.Null, "Start button not found");
            IntPtr startHwnd = (IntPtr)startBtn.Properties.NativeWindowHandle.Value;

            Task click = Task.Run(() => SendMessage(startHwnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero));
            ClickMessageBox("ATTENTION!", "Yes", 15000);
            ClickMessageBox("Folder Error", "OK", 15000);
            click.Wait(20000);

            WaitUntil(() => IsControlEnabled("startBtn"), 10000, "Start button re-enabled");
            Assert.That(FindWindowByTitle("Enjoy!"), Is.EqualTo(IntPtr.Zero), "Enjoy! must not appear when the run aborted");
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

        [Test]
        public void SetCurrentPath_NoFolderArg_FallsBackToDownloads()
        {
            LaunchAppWithoutFolderArg();
            WaitUntilStartEnabled();

            string expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            WaitUntil(() =>
            {
                try
                {
                    return GetTextBoxText("location") == expected;
                }
                catch
                {
                    return false;
                }
            }, 10000, "location shows Downloads folder");
        }

        [Test]
        public void LocationCount_InvalidPath_ResetsCountAndRenameTarget()
        {
            LaunchApp("a.txt");
            WaitUntilStartEnabled();
            WaitUntil(() => LabelText("totalFilesCount") == "1", 10000, "file count shows 1");

            var location = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("location"));
            Assert.That(location, Is.Not.Null, "location textbox not found");
            IntPtr locationHwnd = (IntPtr)location.Properties.NativeWindowHandle.Value;
            SendMessage(locationHwnd, WM_SETTEXT, IntPtr.Zero, "C:\\bad<>path\\x");

            WaitUntil(() =>
            {
                try
                {
                    return LabelText("totalFilesCount") == "0" && GetTextBoxText("renameTo") == "";
                }
                catch
                {
                    return false;
                }
            }, 10000, "count reset to 0 and rename target cleared");
        }

        [Test]
        public void AboutMenu_OpensAndClosesAboutDialog()
        {
            LaunchApp();
            WaitUntilStartEnabled();

            var aboutItem = _mainWindow.FindFirstDescendant(cf => cf.ByName("About"));
            Assert.That(aboutItem, Is.Not.Null, "About menu item not found");
            // UIA Invoke blocks while the modal dialog is open, so click the menu
            // item with the mouse instead.
            System.Drawing.Rectangle rect = aboutItem.Properties.BoundingRectangle.Value;
            Mouse.Click(new System.Drawing.Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2));

            WaitUntil(() => FindWindowByTitle("About Folder Prettifier") != IntPtr.Zero, 15000, "about dialog opened");

            IntPtr aboutHwnd = FindWindowByTitle("About Folder Prettifier");
            Window aboutWindow = _automation.FromHandle(aboutHwnd).AsWindow();
            var okayBtn = aboutWindow.FindFirstDescendant(cf => cf.ByName("Okay").And(cf.ByControlType(ControlType.Button)));
            Assert.That(okayBtn, Is.Not.Null, "Okay button not found in About dialog");
            okayBtn.Patterns.Invoke.Pattern.Invoke();

            WaitUntil(() => FindWindowByTitle("About Folder Prettifier") == IntPtr.Zero, 5000, "about dialog closed");
        }

        [Test]
        public void StartButton_AttentionNo_AbortsWithoutProcessing()
        {
            LaunchApp("a.txt");
            WaitUntilStartEnabled();

            var startBtn = _mainWindow.FindFirstDescendant(cf => cf.ByName("Start").And(cf.ByControlType(ControlType.Button)));
            Assert.That(startBtn, Is.Not.Null, "Start button not found");
            IntPtr startHwnd = (IntPtr)startBtn.Properties.NativeWindowHandle.Value;

            Task click = Task.Run(() => SendMessage(startHwnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero));
            ClickMessageBox("ATTENTION!", "No", 15000);
            click.Wait(20000);

            Assert.That(FindWindowByTitle("Enjoy!"), Is.EqualTo(IntPtr.Zero), "Enjoy! must not appear when the run is declined");
            Assert.That(File.Exists(Path.Combine(_tempDir, "a.txt")), Is.True, "files must be untouched");
            WaitUntil(() => IsControlEnabled("startBtn"), 10000, "Start button enabled");
        }

        [Test]
        public void StartButton_OpenFolder_LaunchesExplorerForFolder()
        {
            LaunchApp("a.txt");
            WaitUntilStartEnabled();
            ToggleCheckBox("Open folder after prettification", true);

            ClickStartAndHandleDialogs();

            WaitUntil(() => FindAnyWindowByTitlePrefix("Source") != IntPtr.Zero, 15000, "explorer window for the folder");
        }

        [Test]
        public void RenameConflict_Yes_DeletesConflictingFolderAndRenames()
        {
            LaunchApp("a.txt");
            WaitUntilStartEnabled();
            ToggleCheckBox("Categorize Files", false);

            string target = Path.Combine(_stageDir, "Target");
            Directory.CreateDirectory(target);
            File.WriteAllText(Path.Combine(target, "keep.txt"), "keep");

            var renameTo = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("renameTo"));
            Assert.That(renameTo, Is.Not.Null, "renameTo textbox not found");
            renameTo.AsTextBox().Text = "Target";

            var startBtn = _mainWindow.FindFirstDescendant(cf => cf.ByName("Start").And(cf.ByControlType(ControlType.Button)));
            Assert.That(startBtn, Is.Not.Null, "Start button not found");
            IntPtr startHwnd = (IntPtr)startBtn.Properties.NativeWindowHandle.Value;

            Task click = Task.Run(() => SendMessage(startHwnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero));
            ClickMessageBox("ATTENTION!", "Yes", 15000);
            ClickMessageBox("Folder Conflict!", "Yes", 15000);
            ClickMessageBox("Enjoy!", "OK", 30000);
            click.Wait(20000);

            Assert.That(Directory.Exists(_tempDir), Is.False, "source folder must be renamed away");
            Assert.That(File.Exists(Path.Combine(target, "a.txt")), Is.True, "files must move into the target folder. State:\n" + DumpTree(_stageDir));
            Assert.That(File.Exists(Path.Combine(target, "keep.txt")), Is.False, "conflicting folder must be deleted");
        }

        [Test]
        public void RenameConflict_No_AbortsWithoutRenamingOrDeleting()
        {
            LaunchApp("a.txt");
            WaitUntilStartEnabled();
            ToggleCheckBox("Categorize Files", false);

            string target = Path.Combine(_stageDir, "Target");
            Directory.CreateDirectory(target);
            File.WriteAllText(Path.Combine(target, "keep.txt"), "keep");

            var renameTo = _mainWindow.FindFirstDescendant(cf => cf.ByAutomationId("renameTo"));
            Assert.That(renameTo, Is.Not.Null, "renameTo textbox not found");
            renameTo.AsTextBox().Text = "Target";

            var startBtn = _mainWindow.FindFirstDescendant(cf => cf.ByName("Start").And(cf.ByControlType(ControlType.Button)));
            Assert.That(startBtn, Is.Not.Null, "Start button not found");
            IntPtr startHwnd = (IntPtr)startBtn.Properties.NativeWindowHandle.Value;

            Task click = Task.Run(() => SendMessage(startHwnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero));
            ClickMessageBox("ATTENTION!", "Yes", 15000);
            ClickMessageBox("Folder Conflict!", "No", 15000);
            click.Wait(20000);

            Assert.That(FindWindowByTitle("Enjoy!"), Is.EqualTo(IntPtr.Zero), "Enjoy! must not appear when the rename is declined");
            Assert.That(Directory.Exists(_tempDir), Is.True, "source folder must be untouched");
            Assert.That(File.Exists(Path.Combine(_tempDir, "a.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(target, "keep.txt")), Is.True, "conflicting folder must be untouched");
            WaitUntil(() => IsControlEnabled("startBtn"), 10000, "Start button re-enabled");
        }

        [Test]
        public void SomeFilesCouldNotBeProcessed_DialogShownForLockedFile()
        {
            LaunchApp("ok.txt");
            WaitUntilStartEnabled();
            ToggleCheckBox("Categorize Files", true);

            string locked = Path.Combine(_tempDir, "locked.txt");
            File.WriteAllText(locked, "locked");

            using (FileStream fs = new FileStream(locked, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var startBtn = _mainWindow.FindFirstDescendant(cf => cf.ByName("Start").And(cf.ByControlType(ControlType.Button)));
                Assert.That(startBtn, Is.Not.Null, "Start button not found");
                IntPtr startHwnd = (IntPtr)startBtn.Properties.NativeWindowHandle.Value;

                Task click = Task.Run(() => SendMessage(startHwnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero));
                ClickMessageBox("ATTENTION!", "Yes", 15000);
                ClickMessageBox("Some files could not be processed", "OK", 30000);
                ClickMessageBox("Enjoy!", "OK", 30000);
                click.Wait(20000);
            }

            Assert.That(File.Exists(locked), Is.True, "locked file must remain in place");
            Assert.That(File.Exists(Path.Combine(_tempDir, "Documents", "Text", "ok.txt")), Is.True, "other files must still be processed");
        }

        [Test]
        public void CheckForUpdates_MenuShowsResultDialogAndReturnsToReady()
        {
            LaunchApp();
            WaitUntilStartEnabled();

            var updateItem = _mainWindow.FindFirstDescendant(cf => cf.ByName("Check for Updates"));
            Assert.That(updateItem, Is.Not.Null, "Check for Updates menu item not found");
            try
            {
                updateItem.Patterns.Invoke.Pattern.Invoke();
            }
            catch
            {
                updateItem.Focus();
                Keyboard.Type(VirtualKeyShort.RETURN);
            }

            // Non-silent check: a result dialog appears whether or not an update exists.
            WaitUntil(() =>
                FindWindowByTitle("Up to date") != IntPtr.Zero ||
                FindWindowByTitle("Update Available") != IntPtr.Zero, 45000, "update result dialog");

            if (FindWindowByTitle("Update Available") != IntPtr.Zero)
            {
                ClickMessageBox("Update Available", "No", 15000);
            }
            else
            {
                ClickMessageBox("Up to date", "OK", 15000);
            }

            WaitUntil(() =>
                _mainWindow.FindFirstDescendant(cf => cf.ByName("Ready")) != null, 15000, "status Ready");
        }
    }
}
