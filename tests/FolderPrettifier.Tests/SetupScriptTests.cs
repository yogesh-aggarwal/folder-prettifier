using NUnit.Framework;
using System;
using System.IO;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class SetupScriptTests
    {
        private static string FindRepoRoot()
        {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "scripts", "setup.iss")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            throw new InvalidOperationException("Could not locate repo root (scripts/setup.iss not found).");
        }

        private static string[] ReadSetupScript()
        {
            return File.ReadAllLines(Path.Combine(FindRepoRoot(), "scripts", "setup.iss"));
        }

        private static string ReadSetupScriptText()
        {
            return string.Join(Environment.NewLine, ReadSetupScript());
        }

        [Test]
        public void ContextMenuTask_IsCheckedByDefault()
        {
            string[] lines = ReadSetupScript();
            foreach (string line in lines)
            {
                if (line.Contains("Name: \"contextmenu\""))
                {
                    StringAssert.Contains("Checked: yes", line,
                        "Context menu task must be checked by default in setup.iss.");
                    return;
                }
            }
            Assert.Fail("contextmenu task not found in setup.iss.");
        }

        [Test]
        public void ContextMenuTask_RegistersFolderAndBackgroundEntries()
        {
            string text = ReadSetupScriptText();
            StringAssert.Contains("Directory\\shell\\FolderPrettifier\\command", text);
            StringAssert.Contains("Directory\\Background\\shell\\FolderPrettifier\\command", text);
        }
    }

    [TestFixture]
    public class SetupDotNetLogicTests
    {
        private static string Text()
        {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "scripts", "setup.iss")))
                    return string.Join(Environment.NewLine, File.ReadAllLines(Path.Combine(dir.FullName, "scripts", "setup.iss")));
                dir = dir.Parent;
            }
            throw new InvalidOperationException("Could not locate repo root (scripts/setup.iss not found).");
        }

        [Test]
        public void InitializeSetup_StartsAssumingDotNetInstalled()
        {
            string text = Text();
            StringAssert.Contains("function InitializeSetup: Boolean;", text);
            StringAssert.Contains("Result := True;", text);
            StringAssert.Contains("ShouldInstallDotNet := False;", text);
        }

        [Test]
        public void InitializeSetup_PromptsAndSetsFlagWhenDotNetMissing()
        {
            string text = Text();
            StringAssert.Contains("if not IsDotNet481Installed then", text);
            StringAssert.Contains("Folder Prettifier requires .NET Framework 4.8.1.", text);
            StringAssert.Contains("mbConfirmation, MB_YESNO", text);
            StringAssert.Contains("ShouldInstallDotNet := True;", text);
            StringAssert.Contains("Result := False;", text);
        }

        [Test]
        public void IsDotNet481Installed_RequiresReleaseKey528040OrHigher()
        {
            string text = Text();
            StringAssert.Contains("function IsDotNet481Installed: Boolean;", text);
            StringAssert.Contains("RegKeyExists(HKLM, 'SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full')", text);
            StringAssert.Contains("RegQueryDWordValue(HKLM, 'SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full', 'Release', version)", text);
            StringAssert.Contains("version >= 528040", text);
        }

        [Test]
        public void CurStepChanged_OnlyInstallsDuringInstallStepWhenFlagSet()
        {
            string text = Text();
            StringAssert.Contains("procedure CurStepChanged(CurStep: TSetupStep);", text);
            StringAssert.Contains("(CurStep = ssInstall) and ShouldInstallDotNet", text);
        }

        [Test]
        public void CurStepChanged_UsesOfficialDotNet481WebDownloader()
        {
            string text = Text();
            StringAssert.Contains("https://go.microsoft.com/fwlink/?linkid=2203306", text);
            StringAssert.Contains("NDP481-Web.exe", text);
            StringAssert.Contains("WizardForm.StatusLabel.Caption := 'Downloading .NET Framework 4.8.1...';", text);
        }

        [Test]
        public void CurStepChanged_InstallsSilentlyWithoutRestart()
        {
            string text = Text();
            StringAssert.Contains("Exec(setupPath, '/q /norestart'", text);
            StringAssert.Contains("WizardForm.StatusLabel.Caption := 'Installing .NET Framework 4.8.1...';", text);
            StringAssert.Contains("Sleep(2000);", text);
        }

        [Test]
        public void CurStepChanged_AbortsWhenInstallStillMissing()
        {
            string text = Text();
            StringAssert.Contains("if not IsDotNet481Installed then", text);
            StringAssert.Contains(".NET Framework 4.8.1 installation failed or was cancelled.", text);
            StringAssert.Contains("Abort;", text);
        }

        [Test]
        public void DownloadFile_UsesPowerShellWebClientAndWaits()
        {
            string text = Text();
            StringAssert.Contains("function DownloadFile(URL, FileName: string): Boolean;", text);
            StringAssert.Contains("New-Object System.Net.WebClient", text);
            StringAssert.Contains("$web.DownloadFile(''' + URL + ''', ''' + FileName + ''')", text);
            StringAssert.Contains("exit 0", text);
            StringAssert.Contains("exit 1", text);
            StringAssert.Contains("SW_HIDE", text);
            StringAssert.Contains("ewWaitUntilTerminated", text);
        }
    }
}
