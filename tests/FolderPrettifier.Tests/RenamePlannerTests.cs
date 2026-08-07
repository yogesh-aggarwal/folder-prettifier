using FolderPrettifier;
using NUnit.Framework;
using System.IO;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class RenamePlannerTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "fpr-test-" + System.Guid.NewGuid().ToString("N"));
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
        public void Plan_ValidTarget_ComputesPathUnderParent()
        {
            string src = Path.Combine(_tempDir, "Source");
            Directory.CreateDirectory(src);

            RenamePlan plan = RenamePlanner.Plan(src, "Renamed");

            Assert.That(plan.TargetPath, Is.EqualTo(Path.Combine(_tempDir, "Renamed")));
            Assert.That(plan.IsRename, Is.True);
            Assert.That(plan.Conflict, Is.False);
        }

        [Test]
        public void Plan_EmptyTarget_NoRename()
        {
            string src = Path.Combine(_tempDir, "Source");
            Directory.CreateDirectory(src);

            RenamePlan plan = RenamePlanner.Plan(src, "");

            Assert.That(plan.IsRename, Is.False);
            Assert.That(plan.TargetPath, Is.Null);
        }

        [Test]
        public void Plan_NullTarget_NoRename()
        {
            string src = Path.Combine(_tempDir, "Source");
            Directory.CreateDirectory(src);

            RenamePlan plan = RenamePlanner.Plan(src, null);

            Assert.That(plan.IsRename, Is.False);
        }

        [Test]
        public void Plan_SameName_NoRename()
        {
            string src = Path.Combine(_tempDir, "Source");
            Directory.CreateDirectory(src);

            RenamePlan plan = RenamePlanner.Plan(src, "Source");

            Assert.That(plan.IsRename, Is.False);
            Assert.That(plan.Conflict, Is.False);
        }

        [Test]
        public void Plan_ExistingTarget_Conflict()
        {
            string src = Path.Combine(_tempDir, "Source");
            string target = Path.Combine(_tempDir, "Existing");
            Directory.CreateDirectory(src);
            Directory.CreateDirectory(target);

            RenamePlan plan = RenamePlanner.Plan(src, "Existing");

            Assert.That(plan.IsRename, Is.True);
            Assert.That(plan.Conflict, Is.True);
        }
    }
}
