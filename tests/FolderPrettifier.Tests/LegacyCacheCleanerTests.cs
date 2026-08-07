using FolderPrettifier;
using NUnit.Framework;
using System;
using System.IO;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class LegacyCacheCleanerTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "fplc-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        [Test]
        public void Clean_RemovesCatFpc()
        {
            string legacy = Path.Combine(_tempDir, "cat.fpc");
            File.WriteAllText(legacy, "old-cache");

            LegacyCacheCleaner.Clean(_tempDir);

            Assert.That(File.Exists(legacy), Is.False);
        }

        [Test]
        public void Clean_MissingFile_NoOp()
        {
            Assert.DoesNotThrow(() => LegacyCacheCleaner.Clean(_tempDir));
        }

        [Test]
        public void Clean_LeavesOtherFilesUntouched()
        {
            string other = Path.Combine(_tempDir, "versions.jsonc");
            File.WriteAllText(other, "data");
            File.WriteAllText(Path.Combine(_tempDir, "cat.fpc"), "old-cache");

            LegacyCacheCleaner.Clean(_tempDir);

            Assert.That(File.Exists(other), Is.True);
        }

        [Test]
        public void Clean_NullDirectory_NoThrow()
        {
            Assert.DoesNotThrow(() => LegacyCacheCleaner.Clean(null));
        }

        [Test]
        public void Clean_EmptyDirectory_NoThrow()
        {
            Assert.DoesNotThrow(() => LegacyCacheCleaner.Clean(""));
        }

        [Test]
        public void Clean_NonexistentDirectory_NoThrow()
        {
            Assert.DoesNotThrow(() => LegacyCacheCleaner.Clean(Path.Combine(_tempDir, "does-not-exist")));
        }
    }
}
