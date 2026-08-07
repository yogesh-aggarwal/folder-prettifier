using FolderPrettifier;
using NUnit.Framework;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class FileNamePrettifierTests
    {
        private static PrettifyOptions AllOptions(bool capitalize = true, bool replace = false, bool nameWith = true)
        {
            return new PrettifyOptions
            {
                Capitalize = capitalize,
                Replace = replace,
                ReplaceFrom = "",
                ReplaceTo = "",
                UseNameWith = nameWith,
                Prefix = "pre",
                Suffix = "post"
            };
        }

        [Test]
        public void Prettify_NoOptions_ReturnsUnchanged()
        {
            PrettifyOptions options = new PrettifyOptions();

            Assert.That(FileNamePrettifier.Prettify("hello.txt", options), Is.EqualTo("hello.txt"));
        }

        [Test]
        public void Prettify_Capitalize_UppercasesFirstLetter()
        {
            PrettifyOptions options = new PrettifyOptions { Capitalize = true };

            Assert.That(FileNamePrettifier.Prettify("hello.txt", options), Is.EqualTo("Hello.txt"));
        }

        [Test]
        public void Prettify_Capitalize_AlreadyUppercaseUnchanged()
        {
            PrettifyOptions options = new PrettifyOptions { Capitalize = true };

            Assert.That(FileNamePrettifier.Prettify("Hello.txt", options), Is.EqualTo("Hello.txt"));
        }

        [Test]
        public void Prettify_Capitalize_EmptyStringUnchanged()
        {
            PrettifyOptions options = new PrettifyOptions { Capitalize = true };

            Assert.That(FileNamePrettifier.Prettify("", options), Is.EqualTo(""));
        }

        [Test]
        public void Prettify_Replace_Found()
        {
            PrettifyOptions options = new PrettifyOptions { Replace = true, ReplaceFrom = "old", ReplaceTo = "new" };

            Assert.That(FileNamePrettifier.Prettify("my-old-file.txt", options), Is.EqualTo("my-new-file.txt"));
        }

        [Test]
        public void Prettify_Replace_AllOccurrences()
        {
            PrettifyOptions options = new PrettifyOptions { Replace = true, ReplaceFrom = "a", ReplaceTo = "x" };

            Assert.That(FileNamePrettifier.Prettify("banana.txt", options), Is.EqualTo("bxnxnx.txt"));
        }

        [Test]
        public void Prettify_Replace_NotFoundUnchanged()
        {
            PrettifyOptions options = new PrettifyOptions { Replace = true, ReplaceFrom = "zzz", ReplaceTo = "new" };

            Assert.That(FileNamePrettifier.Prettify("hello.txt", options), Is.EqualTo("hello.txt"));
        }

        [Test]
        public void Prettify_Replace_EmptyFromString_NoOp()
        {
            PrettifyOptions options = new PrettifyOptions { Replace = true, ReplaceFrom = "", ReplaceTo = "x" };

            Assert.That(FileNamePrettifier.Prettify("abc.txt", options), Is.EqualTo("abc.txt"));
        }

        [Test]
        public void Prettify_NameWith_WithExtension_InsertsBeforeExtension()
        {
            PrettifyOptions options = new PrettifyOptions { UseNameWith = true, Prefix = "pre", Suffix = "post" };

            Assert.That(FileNamePrettifier.Prettify("report.pdf", options), Is.EqualTo("prereportpost.pdf"));
        }

        [Test]
        public void Prettify_NameWith_NoExtension_WrapsWholeName()
        {
            PrettifyOptions options = new PrettifyOptions { UseNameWith = true, Prefix = "pre", Suffix = "post" };

            Assert.That(FileNamePrettifier.Prettify("README", options), Is.EqualTo("preREADMEpost"));
        }

        [Test]
        public void Prettify_NameWith_LeadingDotFile_NoExtensionSplit()
        {
            PrettifyOptions options = new PrettifyOptions { UseNameWith = true, Prefix = "pre", Suffix = "post" };

            Assert.That(FileNamePrettifier.Prettify(".gitignore", options), Is.EqualTo("pre.gitignorepost"));
        }

        [Test]
        public void Prettify_NameWith_MultipleDots_SplitsAtLastDot()
        {
            PrettifyOptions options = new PrettifyOptions { UseNameWith = true, Prefix = "pre", Suffix = "post" };

            Assert.That(FileNamePrettifier.Prettify("file.tar.gz", options), Is.EqualTo("prefile.tarpost.gz"));
        }

        [Test]
        public void Prettify_NameWith_TrailingDot_SplitsAtLastDot()
        {
            PrettifyOptions options = new PrettifyOptions { UseNameWith = true, Prefix = "pre", Suffix = "post" };

            Assert.That(FileNamePrettifier.Prettify("file.", options), Is.EqualTo("prefilepost."));
        }

        [Test]
        public void Prettify_AllOptions_OrderIsCapitalizeThenReplaceThenNameWith()
        {
            PrettifyOptions options = new PrettifyOptions
            {
                Capitalize = true,
                Replace = true,
                ReplaceFrom = "ello",
                ReplaceTo = "i",
                UseNameWith = true,
                Prefix = "pre",
                Suffix = "post"
            };

            Assert.That(FileNamePrettifier.Prettify("hello.txt", options), Is.EqualTo("preHipost.txt"));
        }

        [Test]
        public void Prettify_NullOptions_Throws()
        {
            Assert.That(() => FileNamePrettifier.Prettify("a.txt", null), Throws.ArgumentNullException);
        }

        [Test]
        public void Prettify_NullFileName_Throws()
        {
            Assert.That(() => FileNamePrettifier.Prettify(null, new PrettifyOptions()), Throws.ArgumentNullException);
        }

        [Test]
        public void Sanitize_RemovesInvalidCharacters()
        {
            Assert.That(FileNamePrettifier.Sanitize("a<b>c:d\"e/f\\g|h?i*j"), Is.EqualTo("abcdefghij"));
        }

        [Test]
        public void Sanitize_PreservesValidCharacters()
        {
            Assert.That(FileNamePrettifier.Sanitize("my file (1) - final.txt"), Is.EqualTo("my file (1) - final.txt"));
        }

        [Test]
        public void Sanitize_EmptyStringUnchanged()
        {
            Assert.That(FileNamePrettifier.Sanitize(""), Is.EqualTo(""));
        }

        [Test]
        public void Sanitize_PreservesUnicode()
        {
            Assert.That(FileNamePrettifier.Sanitize("naïve - 日本語.txt"), Is.EqualTo("naïve - 日本語.txt"));
        }

        [Test]
        public void Sanitize_Null_Throws()
        {
            Assert.That(() => FileNamePrettifier.Sanitize(null), Throws.ArgumentNullException);
        }
    }
}
