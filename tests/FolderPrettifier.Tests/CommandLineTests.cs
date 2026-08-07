using FolderPrettifier;
using NUnit.Framework;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class CommandLineTests
    {
        [Test]
        public void TryDispatchUpdater_ValidUpdaterArgs_ReturnsTrue()
        {
            bool dispatched = CommandLine.TryDispatchUpdater(new[] { "--apply-update", @"C:\missing\updater.cmd", "1234" });

            Assert.That(dispatched, Is.True);
        }

        [Test]
        public void TryDispatchUpdater_NoArgs_ReturnsFalse()
        {
            Assert.That(CommandLine.TryDispatchUpdater(new string[0]), Is.False);
        }

        [Test]
        public void TryDispatchUpdater_NullArgs_ReturnsFalse()
        {
            Assert.That(CommandLine.TryDispatchUpdater(null), Is.False);
        }

        [Test]
        public void TryDispatchUpdater_FolderPathArg_ReturnsFalse()
        {
            Assert.That(CommandLine.TryDispatchUpdater(new[] { @"C:\Some\Folder" }), Is.False);
        }

        [Test]
        public void TryDispatchUpdater_WrongFlag_ReturnsFalse()
        {
            Assert.That(CommandLine.TryDispatchUpdater(new[] { "--something-else", @"C:\a.exe", "1" }), Is.False);
        }

        [Test]
        public void TryDispatchUpdater_MissingParts_ReturnsFalse()
        {
            Assert.That(CommandLine.TryDispatchUpdater(new[] { "--apply-update", @"C:\a.exe" }), Is.False);
        }

        [Test]
        public void GetCurrentFolder_FirstArgReturned()
        {
            Assert.That(CommandLine.GetCurrentFolder(new[] { @"C:\My\Folder" }), Is.EqualTo(@"C:\My\Folder"));
        }

        [Test]
        public void GetCurrentFolder_NoArgs_ReturnsEmptyString()
        {
            Assert.That(CommandLine.GetCurrentFolder(new string[0]), Is.EqualTo(""));
        }

        [Test]
        public void GetCurrentFolder_NullArgs_ReturnsEmptyString()
        {
            Assert.That(CommandLine.GetCurrentFolder(null), Is.EqualTo(""));
        }
    }
}
