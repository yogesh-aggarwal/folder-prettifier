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
            string[] lines = ReadSetupScript();
            StringAssert.Contains("Directory\\shell\\FolderPrettifier\\command", string.Join(Environment.NewLine, lines));
            StringAssert.Contains("Directory\\Background\\shell\\FolderPrettifier\\command", string.Join(Environment.NewLine, lines));
        }
    }
}
