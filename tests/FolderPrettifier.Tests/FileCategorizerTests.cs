using FolderPrettifier;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class FileCategorizerTests
    {
        private static Dictionary<string, string> Map(params string[] pairs)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < pairs.Length; i += 2)
            {
                map[pairs[i]] = pairs[i + 1];
            }
            return map;
        }

        [Test]
        public void CategoryFor_KnownExtension_ReturnsFolder()
        {
            Dictionary<string, string> map = Map("mp4", "Videos");

            Assert.That(FileCategorizer.CategoryFor("C:\\x\\movie.MP4", map, "Others\\Unknown"), Is.EqualTo("Videos"));
        }

        [Test]
        public void CategoryFor_UnknownExtension_ReturnsDefault()
        {
            Dictionary<string, string> map = Map("mp4", "Videos");

            Assert.That(FileCategorizer.CategoryFor("C:\\x\\file.pages", map, "Others\\Unknown"), Is.EqualTo("Others\\Unknown"));
        }

        [Test]
        public void CategoryFor_NoExtension_ReturnsDefault()
        {
            Dictionary<string, string> map = Map("mp4", "Videos");

            Assert.That(FileCategorizer.CategoryFor("C:\\x\\README", map, "Others\\Unknown"), Is.EqualTo("Others\\Unknown"));
        }

        [Test]
        public void CategoryFor_CaseInsensitiveLookup()
        {
            Dictionary<string, string> map = Map("PDF", "Documents\\Office\\PDF");

            Assert.That(FileCategorizer.CategoryFor("C:\\x\\doc.pdf", map, "D"), Is.EqualTo("Documents\\Office\\PDF"));
        }

        [Test]
        public void CategoryFor_MultipleDotExtension_UsesLastDot()
        {
            Dictionary<string, string> map = Map("gz", "Compressed", "tar", "Compressed");

            Assert.That(FileCategorizer.CategoryFor("C:\\x\\file.tar.gz", map, "D"), Is.EqualTo("Compressed"));
        }

        [Test]
        public void ResolveDestinationPath_NoCollision_ReturnsPath()
        {
            Assert.That(FileCategorizer.ResolveDestinationPath("C:\\x", "a.txt", p => false),
                Is.EqualTo("C:\\x\\a.txt"));
        }

        [Test]
        public void ResolveDestinationPath_OneCollision_AppendsSuffix()
        {
            Assert.That(FileCategorizer.ResolveDestinationPath("C:\\x", "a.txt", p => p == "C:\\x\\a.txt"),
                Is.EqualTo("C:\\x\\a (1).txt"));
        }

        [Test]
        public void ResolveDestinationPath_MultipleCollisions_IncrementsSuffix()
        {
            Assert.That(FileCategorizer.ResolveDestinationPath("C:\\x", "a.txt",
                p => p == "C:\\x\\a.txt" || p == "C:\\x\\a (1).txt"),
                Is.EqualTo("C:\\x\\a (2).txt"));
        }

        [Test]
        public void ResolveDestinationPath_NoExtension_KeepsSuffixFormat()
        {
            Assert.That(FileCategorizer.ResolveDestinationPath("C:\\x", "README",
                p => p == "C:\\x\\README"),
                Is.EqualTo("C:\\x\\README (1)"));
        }

        [Test]
        public void ResolveDestinationPath_FileNameWithDots_OnlyLastDotIsExtension()
        {
            Assert.That(FileCategorizer.ResolveDestinationPath("C:\\x", "file.tar.gz",
                p => p == "C:\\x\\file.tar.gz"),
                Is.EqualTo("C:\\x\\file.tar (1).gz"));
        }

        [Test]
        public void ResolveDestinationPath_NullPredicate_Throws()
        {
            Assert.That(() => FileCategorizer.ResolveDestinationPath("C:\\x", "a.txt", null),
                Throws.ArgumentNullException);
        }

        [Test]
        public void ResolveDestinationPath_RealFileSystemBehavior()
        {
            string dir = Path.Combine(Path.GetTempPath(), "fpc-test-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "a.txt"), "first");
                string second = FileCategorizer.ResolveDestinationPath(dir, "a.txt", File.Exists);
                File.WriteAllText(second, "second");

                Assert.That(File.Exists(Path.Combine(dir, "a.txt")), Is.True);
                Assert.That(File.Exists(Path.Combine(dir, "a (1).txt")), Is.True);
                Assert.That(second, Is.EqualTo(Path.Combine(dir, "a (1).txt")));
            }
            finally
            {
                Directory.Delete(dir, true);
            }
        }
    }
}
