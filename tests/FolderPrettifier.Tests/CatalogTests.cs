using FolderPrettifier;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class CatalogTests
    {
        private static Catalog BuildCatalog(
            string defaultFolder = "Others/Unknown",
            string minAppVersion = "2.0.1",
            params CatalogCategory[] categories)
        {
            Catalog catalog = new Catalog
            {
                Version = 1,
                MinAppVersion = minAppVersion,
                Default = defaultFolder,
                Categories = new List<CatalogCategory>(categories)
            };
            return catalog;
        }

        private static CatalogCategory Category(string folder, params string[] extensions)
        {
            return new CatalogCategory
            {
                Name = folder,
                Folder = folder,
                Extensions = new List<string>(extensions)
            };
        }

        [Test]
        public void Deserialize_JsoncWithComments_Parses()
        {
            string json = @"{
                // catalog version
                ""version"": 3,
                /* min app */
                ""min-app-version"": ""2.0.1"",
                ""default"": ""Others/Unknown"",
                ""categories"": [
                    { ""name"": ""Videos"", ""folder"": ""Videos"", ""extensions"": [""mp4"", ""mkv""] }
                ]
            }";

            Catalog catalog = Newtonsoft.Json.JsonConvert.DeserializeObject<Catalog>(json);

            Assert.That(catalog.Version, Is.EqualTo(3));
            Assert.That(catalog.MinAppVersion, Is.EqualTo("2.0.1"));
            Assert.That(catalog.Categories.Count, Is.EqualTo(1));
            Assert.That(catalog.Categories[0].Extensions, Is.EquivalentTo(new[] { "mp4", "mkv" }));
        }

        [Test]
        public void BuildExtensionMap_LowercasesKeys()
        {
            Catalog catalog = BuildCatalog(categories: Category("Videos", "MP4", "Mkv", "3GP"));

            Dictionary<string, string> map = catalog.BuildExtensionMap();

            CollectionAssert.Contains(map.Keys, "mp4");
            CollectionAssert.Contains(map.Keys, "mkv");
            CollectionAssert.Contains(map.Keys, "3gp");
            CollectionAssert.DoesNotContain(map.Keys, "MP4");
        }

        [Test]
        public void BuildExtensionMap_LookupIsCaseInsensitive()
        {
            Catalog catalog = BuildCatalog(categories: Category("Videos", "mp4"));

            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map["MP4"], Is.EqualTo("Videos"));
            Assert.That(map["Mp4"], Is.EqualTo("Videos"));
        }

        [Test]
        public void BuildExtensionMap_TrimsLeadingDots()
        {
            Catalog catalog = BuildCatalog(categories: Category("Images", ".jpg", "..png"));

            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map.ContainsKey("jpg"), Is.True);
            Assert.That(map.ContainsKey("png"), Is.True);
        }

        [Test]
        public void BuildExtensionMap_DuplicateExtension_LastCategoryWins()
        {
            Catalog catalog = BuildCatalog("Others/Unknown", "2.0.1",
                Category("Videos", "mp4"),
                Category("Other", "mp4", "mp3"));

            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map["mp4"], Is.EqualTo("Other"));
        }

        [Test]
        public void BuildExtensionMap_NormalizesFolderSlashesInValues()
        {
            Catalog catalog = BuildCatalog(categories: Category("Documents/Office/Word", "docx"));

            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map["docx"],
                Is.EqualTo("Documents" + System.IO.Path.DirectorySeparatorChar + "Office" + System.IO.Path.DirectorySeparatorChar + "Word"));
        }

        [Test]
        public void BuildExtensionMap_SkipsCategoryWithEmptyFolder()
        {
            Catalog catalog = BuildCatalog("Others/Unknown", "2.0.1",
                Category("", "mp4"),
                Category(null, "mkv"),
                Category("Videos", "avi"));

            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map.Count, Is.EqualTo(1));
            Assert.That(map["avi"], Is.EqualTo("Videos"));
        }

        [Test]
        public void BuildExtensionMap_NullOrEmptyExtensionsIgnored()
        {
            Catalog catalog = BuildCatalog(categories: Category("Videos", "mp4", "", null));

            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map.Count, Is.EqualTo(1));
        }

        [Test]
        public void BuildExtensionMap_NullCategoriesReturnedEmpty()
        {
            Catalog catalog = BuildCatalog();

            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map, Is.Empty);
        }

        [Test]
        public void DefaultFolder_ConvertsForwardSlashesToPlatformSeparator()
        {
            Catalog catalog = BuildCatalog(defaultFolder: "Others/Unknown", categories: Category("Videos", "mp4"));

            Assert.That(catalog.DefaultFolder, Is.EqualTo("Others" + System.IO.Path.DirectorySeparatorChar + "Unknown"));
        }

        [Test]
        public void DefaultFolder_WhenNull_UsesOthersUnknown()
        {
            Catalog catalog = BuildCatalog(defaultFolder: null, categories: Category("Videos", "mp4"));

            Assert.That(catalog.DefaultFolder, Does.EndWith("Unknown"));
        }

        [Test]
        public void DefaultFolder_WhenEmpty_UsesOthersUnknown()
        {
            Catalog catalog = BuildCatalog(defaultFolder: "", categories: Category("Videos", "mp4"));

            Assert.That(catalog.DefaultFolder, Does.EndWith("Unknown"));
        }

        [Test]
        public void IsCompatibleWith_AppVersionEqualToMin_True()
        {
            Catalog catalog = BuildCatalog(minAppVersion: "2.0.1", categories: Category("Videos", "mp4"));

            Assert.That(catalog.IsCompatibleWith(new Version(2, 0, 1)), Is.True);
        }

        [Test]
        public void IsCompatibleWith_AppVersionNewerThanMin_True()
        {
            Catalog catalog = BuildCatalog(minAppVersion: "2.0.1", categories: Category("Videos", "mp4"));

            Assert.That(catalog.IsCompatibleWith(new Version(2, 5, 0)), Is.True);
        }

        [Test]
        public void IsCompatibleWith_AppVersionOlderThanMin_False()
        {
            Catalog catalog = BuildCatalog(minAppVersion: "2.3.4", categories: Category("Videos", "mp4"));

            Assert.That(catalog.IsCompatibleWith(new Version(2, 0, 1)), Is.False);
        }

        [Test]
        public void IsCompatibleWith_UnparseableMinAppVersion_False()
        {
            Catalog catalog = BuildCatalog(minAppVersion: "not-a-version", categories: Category("Videos", "mp4"));

            Assert.That(catalog.IsCompatibleWith(new Version(9, 9, 9)), Is.False);
        }

        [Test]
        public void IsCompatibleWith_NullMinAppVersion_False()
        {
            Catalog catalog = BuildCatalog(minAppVersion: null, categories: Category("Videos", "mp4"));

            Assert.That(catalog.IsCompatibleWith(new Version(9, 9, 9)), Is.False);
        }
    }
}
