using FolderPrettifier;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class CatalogSelectorTests
    {
        private const string VersionsJson =
            "{ \"2.0.1\": \"v0001.jsonc\", \"2.3.4\": \"v0002.jsonc\" }";

        [Test]
        public void Select_ExactMatch_SelectsThatCatalog()
        {
            CatalogSelection selection = CatalogSelector.Select(VersionsJson, new Version(2, 0, 1));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.Selected));
            Assert.That(selection.FileName, Is.EqualTo("v0001.jsonc"));
        }

        [Test]
        public void Select_AppInRange_SelectsLargestKeyAtOrBelow()
        {
            CatalogSelection selection = CatalogSelector.Select(VersionsJson, new Version(2, 2, 0));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.Selected));
            Assert.That(selection.FileName, Is.EqualTo("v0001.jsonc"));
        }

        [Test]
        public void Select_AppAboveAllKeys_SelectsNewestCatalog()
        {
            CatalogSelection selection = CatalogSelector.Select(VersionsJson, new Version(5, 0, 0));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.Selected));
            Assert.That(selection.FileName, Is.EqualTo("v0002.jsonc"));
        }

        [Test]
        public void Select_AppOlderThanEveryKey_NoMatch()
        {
            CatalogSelection selection = CatalogSelector.Select(VersionsJson, new Version(2, 0, 0));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.NoMatch));
            Assert.That(selection.FileName, Is.Null);
        }

        [Test]
        public void Select_UnparseableVersionKeysAreIgnored()
        {
            string json = "{ \"abc\": \"v0001.jsonc\", \"2.0.1\": \"v0002.jsonc\" }";

            CatalogSelection selection = CatalogSelector.Select(json, new Version(2, 0, 0));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.NoMatch));
        }

        [Test]
        public void Select_OnlyUnparseableKeys_NoMatch()
        {
            string json = "{ \"abc\": \"v0001.jsonc\" }";

            CatalogSelection selection = CatalogSelector.Select(json, new Version(9, 0, 0));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.NoMatch));
        }

        [Test]
        public void Select_UnsortedKeys_StillPicksMaxAtOrBelow()
        {
            string json = "{ \"2.3.4\": \"v0002.jsonc\", \"2.0.1\": \"v0001.jsonc\" }";

            CatalogSelection selection = CatalogSelector.Select(json, new Version(2, 1, 0));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.Selected));
            Assert.That(selection.FileName, Is.EqualTo("v0001.jsonc"));
        }

        [Test]
        public void Select_NullVersionsJson_NoIndex()
        {
            CatalogSelection selection = CatalogSelector.Select(null, new Version(2, 0, 1));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.NoIndex));
        }

        [Test]
        public void Select_EmptyVersionsJson_NoIndex()
        {
            CatalogSelection selection = CatalogSelector.Select("", new Version(2, 0, 1));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.NoIndex));
        }

        [Test]
        public void Select_CorruptVersionsJson_NoIndex()
        {
            CatalogSelection selection = CatalogSelector.Select("{ this is not json", new Version(2, 0, 1));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.NoIndex));
        }

        [Test]
        public void Select_NullAppVersion_Throws()
        {
            Assert.That(() => CatalogSelector.Select(VersionsJson, null), Throws.ArgumentNullException);
        }

        [Test]
        public void Select_EmptyIndexObject_NoIndex()
        {
            CatalogSelection selection = CatalogSelector.Select("{ }", new Version(2, 0, 1));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.NoIndex));
        }

        [Test]
        public void Select_JsoncWithComments_Parses()
        {
            string json = @"{
                // min app version -> catalog file
                ""2.0.1"": ""v0001.jsonc""
            }";

            CatalogSelection selection = CatalogSelector.Select(json, new Version(2, 0, 1));

            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.Selected));
            Assert.That(selection.FileName, Is.EqualTo("v0001.jsonc"));
        }

        [Test]
        public void ParseIndex_ReturnsDictionary()
        {
            Dictionary<string, string> index = CatalogSelector.ParseIndex(VersionsJson);

            Assert.That(index, Is.Not.Null);
            Assert.That(index.Count, Is.EqualTo(2));
            Assert.That(index["2.0.1"], Is.EqualTo("v0001.jsonc"));
        }

        [Test]
        public void ParseIndex_CorruptJson_ReturnsNull()
        {
            Assert.That(CatalogSelector.ParseIndex("not json"), Is.Null);
        }

        [Test]
        public void ParseIndex_Null_ReturnsNull()
        {
            Assert.That(CatalogSelector.ParseIndex(null), Is.Null);
        }
    }
}
