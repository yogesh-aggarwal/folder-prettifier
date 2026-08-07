using FolderPrettifier;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class AdversarialRenamePlannerTests
    {
        private string _stage;

        [SetUp]
        public void SetUp()
        {
            _stage = Path.Combine(Path.GetTempPath(), "fpa-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_stage))
            {
                Directory.Delete(_stage, true);
            }
        }

        private string CreateSource(string name)
        {
            string src = Path.Combine(_stage, name);
            Directory.CreateDirectory(src);
            return src;
        }

        [Test]
        public void Plan_DotTarget_MustNotResolveToParentFolder()
        {
            string parent = _stage;
            string src = CreateSource("Source");

            RenamePlan plan = RenamePlanner.Plan(src, ".");

            Assert.That(plan.IsRename, Is.False,
                "Renaming a folder to '.' must be a no-op; it must never resolve to the parent folder.");
            Assert.That(plan.Conflict, Is.False,
                "A '.' target must never be flagged for deletion.");
            Assert.That(plan.TargetPath, Is.Not.EqualTo(parent),
                "The parent folder must never become the rename/delete target.");
        }

        [Test]
        public void Plan_DoubleDotTarget_MustNotResolveToGrandparentFolder()
        {
            string parent = _stage;
            string src = CreateSource("Source");
            string grandparent = Path.GetDirectoryName(parent);

            RenamePlan plan = RenamePlanner.Plan(src, "..");

            Assert.That(plan.IsRename, Is.False,
                "Renaming a folder to '..' must be a no-op; it must never resolve to the grandparent folder.");
            Assert.That(plan.Conflict, Is.False,
                "A '..' target must never be flagged for deletion.");
            Assert.That(plan.TargetPath, Is.Not.EqualTo(grandparent),
                "The grandparent folder must never become the rename/delete target.");
        }

        [Test]
        public void Plan_CaseOnlyVariant_MustNotBeAConflict()
        {
            string src = CreateSource("Source");

            RenamePlan plan = RenamePlanner.Plan(src, "source");

            Assert.That(plan.IsRename, Is.True,
                "Case-only renames are legitimate on Windows (the folder is not moving).");
            Assert.That(plan.Conflict, Is.False,
                "A case-only target is the SAME folder on a case-insensitive filesystem; " +
                "it must not be flagged for deletion (the app would delete the folder it is processing).");
        }

        [Test]
        public void Plan_TrailingDotTarget_MustNotBeAConflict()
        {
            string src = CreateSource("Source");

            RenamePlan plan = RenamePlanner.Plan(src, "Source.");

            Assert.That(plan.Conflict, Is.False,
                "Windows strips trailing dots; 'Source.' resolves to the folder itself and must not be flagged for deletion.");
        }

        [Test]
        public void Plan_TrailingSpaceTarget_MustNotBeAConflict()
        {
            string src = CreateSource("Source");

            RenamePlan plan = RenamePlanner.Plan(src, "Source ");

            Assert.That(plan.Conflict, Is.False,
                "Windows strips trailing spaces; 'Source ' resolves to the folder itself and must not be flagged for deletion.");
        }

        [Test]
        public void Sanitize_StripsPathSeparatorsAndColon_BlocksPathEscape()
        {
            Assert.That(FileNamePrettifier.Sanitize(@"C:\Windows"), Is.EqualTo("CWindows"));
            Assert.That(FileNamePrettifier.Sanitize(@"..\..\Evil"), Is.EqualTo("....Evil"));
        }
    }

    [TestFixture]
    public class AdversarialFileProcessingTests
    {
        private string _tempDir;

        private static Dictionary<string, string> Map(params string[] pairs)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < pairs.Length; i += 2)
            {
                map[pairs[i]] = pairs[i + 1];
            }
            return map;
        }

        private static ProcessingOptions PrettifyOnly()
        {
            return new ProcessingOptions
            {
                PrettifyOn = true,
                Prettify = new PrettifyOptions { Capitalize = true }
            };
        }

        private static ProcessingOptions CategorizeOnly()
        {
            return new ProcessingOptions { CategorizeOn = true };
        }

        private string CreateFile(string name)
        {
            string path = Path.Combine(_tempDir, name);
            File.WriteAllText(path, "content-" + name);
            return path;
        }

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "fpa-fp-" + Guid.NewGuid().ToString("N"));
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

        // Invariant: every original file survives, content-identical, under its
        // original name (renames/moves only). Comparison is name+content based so
        // legitimate moves into category folders do not trip it.
        private void AssertInvariantNoDataLoss(IEnumerable<KeyValuePair<string, string>> expected)
        {
            string[] expectedPairs = expected
                .OrderBy(x => x.Key + "|" + x.Value).Select(x => x.Key + "|" + x.Value).ToArray();
            string[] actualPairs = Directory.GetFiles(_tempDir, "*", SearchOption.AllDirectories)
                .Select(f => new { Name = Path.GetFileName(f), Content = File.ReadAllText(f) })
                .OrderBy(x => x.Name + "|" + x.Content).Select(x => x.Name + "|" + x.Content).ToArray();

            Assert.That(actualPairs, Is.EquivalentTo(expectedPairs),
                "Every user file must survive processing with identical content.");
        }

        private void AssertInvariantNoDataLoss(string[] expectedFiles)
        {
            AssertInvariantNoDataLoss(expectedFiles.Select(f =>
                new KeyValuePair<string, string>(Path.GetFileName(f), File.ReadAllText(Path.Combine(_tempDir, f)))));
        }

        private KeyValuePair<string, string>[] Snapshot(params string[] files)
        {
            return files.Select(f =>
                new KeyValuePair<string, string>(Path.GetFileName(f), File.ReadAllText(Path.Combine(_tempDir, f)))).ToArray();
        }

        [Test]
        public void Prettify_AlreadyPrettifiedName_IsIdempotentAndErrorFree()
        {
            string file = CreateFile("Hello.txt");

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, new[] { file }, PrettifyOnly(), Map(), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty,
                "Re-prettifying an already prettified name must not raise errors (File.Move same path).");
            AssertInvariantNoDataLoss(new[] { "Hello.txt" });
        }

        [Test]
        public void Prettify_CollidingPrettifiedNames_NeverOverwrite()
        {
            string a = CreateFile("my_file.txt");
            string b = CreateFile("My File.txt");
            string[] files = { a, b };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, PrettifyOnly(), Map(), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty, "Capitalize only upper-cases the first char; these names do not collide.");
            AssertInvariantNoDataLoss(new[] { "My_file.txt", "My File.txt" });
        }

        [Test]
        public void Prettify_CaseOnlyVariant_RenamesCaseSafelyAndIsIdempotent()
        {
            // On NTFS a case-only rename is legal and safe; content must survive.
            string file = CreateFile("b.JPG");

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, new[] { file }, PrettifyOnly(), Map(), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            AssertInvariantNoDataLoss(new[] { "B.JPG" });

            string[] topLevel = Directory.GetFiles(_tempDir);
            FileProcessingResult second = FileProcessor.ProcessFiles(_tempDir, topLevel, PrettifyOnly(), Map(), "Others\\Unknown");
            Assert.That(second.Errors, Is.Empty, "Second run on the prettified name must be a no-op.");
            AssertInvariantNoDataLoss(new[] { "B.JPG" });
        }

        [Test]
        public void Categorize_PrettifyCollisionWithSuffix_NoOverwrite()
        {
            string a = CreateFile("b.txt");
            string b = CreateFile("b (1).txt");
            string[] files = { a, b };
            KeyValuePair<string, string>[] snapshot = Snapshot("b.txt", "b (1).txt");

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, CategorizeOnly(), Map("txt", "Text"), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            AssertInvariantNoDataLoss(snapshot);
        }

        [Test]
        public void Categorize_FileNamedLikeCategoryFolder_NoDataLoss()
        {
            CreateFile("Videos");
            string movie = CreateFile("movie.mp4");
            string[] files = { Path.Combine(_tempDir, "Videos"), movie };
            KeyValuePair<string, string>[] snapshot = Snapshot("Videos", "movie.mp4");

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, CategorizeOnly(), Map("mp4", "Videos"), "Others\\Unknown");

            AssertInvariantNoDataLoss(snapshot);
        }

        [Test]
        public void Categorize_UnicodeEmojiAndPunctuation_NoDataLoss()
        {
            string[] names =
            {
                "héllo wörld 😀.txt",
                "100% & $pecial! [test] (1).pdf",
                ".hidden",
                "file.with.many.dots.tar.gz",
                "noext",
                "UPPERCASE.EXE"
            };
            string[] files = names.Select(CreateFile).ToArray();
            KeyValuePair<string, string>[] snapshot = Snapshot(names);

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, CategorizeOnly(), Map("txt", "Text"), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            AssertInvariantNoDataLoss(snapshot);
        }

        [Test]
        public void Idempotency_ProcessingTwice_IsDeterministicAndSafe()
        {
            string a = CreateFile("a.mp4");
            string b = CreateFile("b.txt");
            string[] files = { a, b };
            KeyValuePair<string, string>[] snapshot = Snapshot("a.mp4", "b.txt");

            FileProcessor.ProcessFiles(_tempDir, files, CategorizeOnly(), Map("mp4", "Videos", "txt", "Text"), "Others\\Unknown");
            string[] afterFirst = Directory.GetFiles(_tempDir, "*", SearchOption.AllDirectories)
                .Select(f => f.Substring(_tempDir.Length + 1)).OrderBy(x => x).ToArray();

            string[] topLevel = Directory.GetFiles(_tempDir);
            FileProcessor.ProcessFiles(_tempDir, topLevel, CategorizeOnly(), Map("mp4", "Videos", "txt", "Text"), "Others\\Unknown");
            string[] afterSecond = Directory.GetFiles(_tempDir, "*", SearchOption.AllDirectories)
                .Select(f => f.Substring(_tempDir.Length + 1)).OrderBy(x => x).ToArray();

            Assert.That(afterSecond, Is.EqualTo(afterFirst), "Processing twice must not mutate the result.");
            AssertInvariantNoDataLoss(snapshot);
        }
    }
}
