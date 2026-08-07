using FolderPrettifier;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class EmbeddedCatalogTests
    {
        private static string GetBasicCatalog()
        {
            ResourceManager rm = new ResourceManager("FolderPrettifier.Data", Assembly.GetExecutingAssembly());
            return rm.GetString("BasicCatalog");
        }

        [Test]
        public void BasicCatalog_ResourceExistsAndIsNotEmpty()
        {
            string basic = GetBasicCatalog();

            Assert.That(basic, Is.Not.Null.And.Not.Empty);
        }

        [Test]
        public void BasicCatalog_ParsesAsCatalog()
        {
            Catalog catalog = Newtonsoft.Json.JsonConvert.DeserializeObject<Catalog>(GetBasicCatalog());

            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.Categories, Is.Not.Null.And.Not.Empty);
            Assert.That(catalog.Version, Is.GreaterThan(0));
        }

        [Test]
        public void BasicCatalog_IsCompatibleWithCurrentAppVersion()
        {
            Catalog catalog = Newtonsoft.Json.JsonConvert.DeserializeObject<Catalog>(GetBasicCatalog());

            Assert.That(catalog.IsCompatibleWith(new Version(2, 1, 0)), Is.True);
        }

        [Test]
        public void BasicCatalog_DefaultFolderIsOthersUnknown()
        {
            Catalog catalog = Newtonsoft.Json.JsonConvert.DeserializeObject<Catalog>(GetBasicCatalog());

            Assert.That(catalog.DefaultFolder,
                Is.EqualTo("Others" + System.IO.Path.DirectorySeparatorChar + "Unknown"));
        }

        [Test]
        public void BasicCatalog_MapsKnownExtensions()
        {
            Catalog catalog = Newtonsoft.Json.JsonConvert.DeserializeObject<Catalog>(GetBasicCatalog());
            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map["mp4"],
                Is.EqualTo("Videos"));
            Assert.That(map["pdf"],
                Is.EqualTo("Documents" + System.IO.Path.DirectorySeparatorChar + "Office" + System.IO.Path.DirectorySeparatorChar + "PDF"));
        }

        [Test]
        public void BasicCatalog_MapsPagesDocuments()
        {
            Catalog catalog = Newtonsoft.Json.JsonConvert.DeserializeObject<Catalog>(GetBasicCatalog());
            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map["pages"],
                Is.EqualTo("Documents" + System.IO.Path.DirectorySeparatorChar + "Office"));
        }

        [Test]
        public void BasicCatalog_LookupWorksForMixedCaseExtensions()
        {
            Catalog catalog = Newtonsoft.Json.JsonConvert.DeserializeObject<Catalog>(GetBasicCatalog());
            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map["JPG"], Is.EqualTo("Images"));
            Assert.That(map["Pages"], Is.EqualTo("Documents" + System.IO.Path.DirectorySeparatorChar + "Office"));
        }

        [Test]
        public void BasicCatalog_HasNoNullOrEmptyFoldersInMap()
        {
            Catalog catalog = Newtonsoft.Json.JsonConvert.DeserializeObject<Catalog>(GetBasicCatalog());
            Dictionary<string, string> map = catalog.BuildExtensionMap();

            CollectionAssert.AllItemsAreNotNull(map.Values);
            CollectionAssert.DoesNotContain(map.Values, "");
        }

        [Test]
        public void BasicCatalog_UnparseableFallback_ReturnsNoCatalog()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(new Version(2, 1, 0), "corrupt index", null, GetBasicCatalog());

            Assert.That(result.Catalog, Is.Not.Null);
        }
    }
}
