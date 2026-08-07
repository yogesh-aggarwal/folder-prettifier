using FolderPrettifier;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class FileProcessorTests
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
            File.WriteAllText(path, "content");
            return path;
        }

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "fpr-test-" + Guid.NewGuid().ToString("N"));
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
        public void ProcessFiles_Prettify_RenamesInPlace()
        {
            string file = CreateFile("hello.txt");
            string[] files = { file };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, PrettifyOnly(), Map(), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            Assert.That(result.Processed, Is.EqualTo(1));
            Assert.That(File.Exists(Path.Combine(_tempDir, "Hello.txt")), Is.True);
        }

        [Test]
        public void ProcessFiles_Prettify_ReplaceChangesNameCompletely()
        {
            string file = CreateFile("my-old.txt");
            string[] files = { file };
            ProcessingOptions options = new ProcessingOptions
            {
                PrettifyOn = true,
                Prettify = new PrettifyOptions { Replace = true, ReplaceFrom = "old", ReplaceTo = "new" }
            };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, options, Map(), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            Assert.That(File.Exists(file), Is.False);
            Assert.That(File.Exists(Path.Combine(_tempDir, "my-new.txt")), Is.True);
        }

        [Test]
        public void ProcessFiles_Categorize_MovesToSubfolder()
        {
            string file = CreateFile("movie.mp4");
            string[] files = { file };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, CategorizeOnly(), Map("mp4", "Videos"), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Videos", "movie.mp4")), Is.True);
            Assert.That(File.Exists(file), Is.False);
        }

        [Test]
        public void ProcessFiles_Categorize_UnknownExtension_GoesToDefault()
        {
            string file = CreateFile("doc.pages");
            string[] files = { file };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, CategorizeOnly(), Map("mp4", "Videos"), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Others", "Unknown", "doc.pages")), Is.True);
        }

        [Test]
        public void ProcessFiles_Categorize_NoExtension_GoesToDefault()
        {
            string file = CreateFile("README");
            string[] files = { file };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, CategorizeOnly(), Map("mp4", "Videos"), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Others", "Unknown", "README")), Is.True);
        }

        [Test]
        public void ProcessFiles_Categorize_SameNameCollision_GetsSuffix()
        {
            // Pre-seed a file in the destination category folder that collides with a source file
            Directory.CreateDirectory(Path.Combine(_tempDir, "Text"));
            File.WriteAllText(Path.Combine(_tempDir, "Text", "b.txt"), "existing");
            string file = CreateFile("b.txt");
            string[] files = { file };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, CategorizeOnly(), Map("txt", "Text"), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Text", "b.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Text", "b (1).txt")), Is.True);
        }

        [Test]
        public void ProcessFiles_CombinedPrettifyAndCategorize()
        {
            string file = CreateFile("movie.mp4");
            string[] files = { file };

            ProcessingOptions options = new ProcessingOptions
            {
                PrettifyOn = true,
                CategorizeOn = true,
                Prettify = new PrettifyOptions { Capitalize = true }
            };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, options, Map("mp4", "Videos"), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Videos", "Movie.mp4")), Is.True);
        }

        [Test]
        public void ProcessFiles_MissingFile_RecordsErrorAndContinues()
        {
            string good = CreateFile("ok.txt");
            string missing = Path.Combine(_tempDir, "ghost.txt");
            string[] files = { missing, good };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, CategorizeOnly(), Map("txt", "Text"), "Others\\Unknown");

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0], Does.StartWith("Failed to categorize"));
            Assert.That(result.Processed, Is.EqualTo(2));
            Assert.That(File.Exists(Path.Combine(_tempDir, "Text", "ok.txt")), Is.True);
        }

        [Test]
        public void ProcessFiles_Prettify_MissingFile_RecordsErrorAndContinues()
        {
            string good = CreateFile("ok.txt");
            string missing = Path.Combine(_tempDir, "ghost.txt");
            string[] files = { missing, good };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, PrettifyOnly(), Map(), "Others\\Unknown");

            Assert.That(result.Errors.Count, Is.EqualTo(1));
            Assert.That(result.Errors[0], Does.StartWith("Failed to prettify"));
            Assert.That(result.Errors[0], Does.Contain("ghost.txt"));
            Assert.That(result.Processed, Is.EqualTo(1), "Failed prettify skips the file entirely (continue).");
            Assert.That(File.Exists(Path.Combine(_tempDir, "Ok.txt")), Is.True);
        }

        [Test]
        public void ProcessFiles_NoOperations_NoChangesNoErrors()
        {
            string file = CreateFile("a.txt");
            string[] files = { file };

            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, files, new ProcessingOptions(), Map(), "Others\\Unknown");

            Assert.That(result.Errors, Is.Empty);
            Assert.That(result.Processed, Is.EqualTo(1));
            Assert.That(File.Exists(file), Is.True);
        }

        [Test]
        public void ProcessFiles_NullFiles_EmptyResult()
        {
            FileProcessingResult result = FileProcessor.ProcessFiles(_tempDir, null, new ProcessingOptions(), Map(), "D");

            Assert.That(result.Processed, Is.EqualTo(0));
            Assert.That(result.Errors, Is.Empty);
        }

        [Test]
        public void ProcessFiles_OnFileCallback_InvokedPerFile()
        {
            string file = CreateFile("a.txt");
            int calls = 0;
            string last = null;

            FileProcessor.ProcessFiles(_tempDir, new[] { file }, new ProcessingOptions(), Map(), "D", f => { calls++; last = f; });

            Assert.That(calls, Is.EqualTo(1));
            Assert.That(last, Is.EqualTo(file));
        }

        [Test]
        public void ApplyRename_ValidPlan_MovesDirectory()
        {
            string src = Path.Combine(_tempDir, "Source");
            Directory.CreateDirectory(src);
            File.WriteAllText(Path.Combine(src, "inner.txt"), "x");

            RenamePlan plan = RenamePlanner.Plan(src, "Renamed");

            Assert.That(FileProcessor.ApplyRename(src, plan), Is.True);
            Assert.That(Directory.Exists(src), Is.False);
            Assert.That(File.Exists(Path.Combine(_tempDir, "Renamed", "inner.txt")), Is.True);
        }

        [Test]
        public void ApplyRename_NoRenamePlan_ReturnsFalse()
        {
            string src = Path.Combine(_tempDir, "Source");
            Directory.CreateDirectory(src);

            Assert.That(FileProcessor.ApplyRename(src, new RenamePlan()), Is.False);
        }

        [Test]
        public void ApplyRename_NullPlan_ReturnsFalse()
        {
            Assert.That(FileProcessor.ApplyRename(_tempDir, null), Is.False);
        }
    }
}
