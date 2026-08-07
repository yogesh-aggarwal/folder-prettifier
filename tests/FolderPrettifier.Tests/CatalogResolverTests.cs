using FolderPrettifier;
using NUnit.Framework;
using System;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class CatalogResolverTests
    {
        private const string VersionsJson =
            "{ \"2.0.1\": \"v0001.jsonc\", \"2.3.4\": \"v0002.jsonc\" }";

        private static readonly Version App = new Version(2, 1, 0);

        private static string CatalogJson(string minAppVersion = "2.0.1", string defaultFolder = "Others/Unknown")
        {
            return "{\"version\": 1, \"min-app-version\": \"" + minAppVersion + "\", \"default\": \"" + defaultFolder +
                "\", \"categories\": [ { \"name\": \"Videos\", \"folder\": \"Videos\", \"extensions\": [\"mp4\"] } ] }";
        }

        [Test]
        public void Resolve_SelectedCatalogCompatible_UsesIt()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, VersionsJson, CatalogJson(), CatalogJson("2.0.0"));

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(result.Catalog.MinAppVersion, Is.EqualTo("2.0.1"));
        }

        [Test]
        public void Resolve_SelectedCatalogIncompatible_DiscardsAndUsesEmbedded()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, VersionsJson, CatalogJson("9.9.9"), CatalogJson("2.0.0"));

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(result.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }

        [Test]
        public void Resolve_SelectedCatalogCorrupt_FallsBackToEmbedded()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, VersionsJson, "not a json", CatalogJson("2.0.0"));

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(result.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }

        [Test]
        public void Resolve_SelectedCatalogMissing_FallsBackToEmbedded()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, VersionsJson, null, CatalogJson("2.0.0"));

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(result.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }

        [Test]
        public void Resolve_NoIndex_UsesEmbedded()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, null, null, CatalogJson("2.0.0"));

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(result.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }

        [Test]
        public void Resolve_CorruptIndex_UsesEmbedded()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, "corrupt", null, CatalogJson("2.0.0"));

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(result.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }

        [Test]
        public void Resolve_AppOlderThanAllIndexEntries_UpdateRequired_NoEmbeddedFallback()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(new Version(1, 5, 0), VersionsJson, null, CatalogJson("1.0.0"));

            Assert.That(result.UpdateRequired, Is.True);
            Assert.That(result.Catalog, Is.Null);
        }

        [Test]
        public void Resolve_EmbeddedIncompatibleWithApp_NoCatalog()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, null, null, CatalogJson("9.9.9"));

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Null);
        }

        [Test]
        public void Resolve_EverythingMissing_NoCatalogNoUpdateRequired()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, null, null, null);

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Null);
        }

        [Test]
        public void Resolve_EmbeddedCorrupt_NoCatalog()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, null, null, "corrupt");

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Null);
        }

        [Test]
        public void Resolve_SelectedCatalogWrongShape_FallsBackToEmbedded()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, VersionsJson, "[]", CatalogJson("2.0.0"));

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(result.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }

        [Test]
        public void Resolve_SelectedAndEmbeddedBothCorrupt_NoCatalog()
        {
            CatalogLoadResult result = CatalogResolver.Resolve(App, VersionsJson, "corrupt", "corrupt");

            Assert.That(result.UpdateRequired, Is.False);
            Assert.That(result.Catalog, Is.Null);
        }

        [Test]
        public void Resolve_EmbeddedJsoncWithComments_Parses()
        {
            string embedded = "{\n// comment\n\"version\": 1, \"min-app-version\": \"2.0.0\", \"default\": \"Others/Unknown\", \"categories\": []\n}";

            CatalogLoadResult result = CatalogResolver.Resolve(App, null, null, embedded);

            Assert.That(result.Catalog, Is.Not.Null);
            Assert.That(result.Catalog.MinAppVersion, Is.EqualTo("2.0.0"));
        }
    }
}
