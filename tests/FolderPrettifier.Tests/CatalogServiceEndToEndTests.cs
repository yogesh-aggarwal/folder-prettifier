using FolderPrettifier;
using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using System.Resources;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class CatalogServiceEndToEndTests
    {
        private static string ResourceString(string name)
        {
            ResourceManager rm = new ResourceManager("FolderPrettifier.Data", Assembly.GetExecutingAssembly());
            return rm.GetString(name);
        }

        private static string GetBasicCatalog()
        {
            return ResourceString("BasicCatalog");
        }

        [Test]
        public void LoadAsync_RealNetworkAndRealEmbeddedCatalog_AlwaysLoads()
        {
            // End-to-end through the exact production wiring: real URLs, real
            // RemoteFileFetcher (real HttpClient), real embedded BasicCatalog.
            // Every path converges on a loaded catalog:
            //   online + files on GitHub -> fetched catalog
            //   online + 404 (not pushed) -> embedded
            //   offline                -> cache, then embedded
            // Only a broken embedded resx or a version-index ahead of the app fails this.
            string cacheDir = Path.Combine(Path.GetTempPath(), "fp-e2e-" + Guid.NewGuid().ToString("N"));
            try
            {
                RemoteFileFetcher fetcher = new RemoteFileFetcher();
                CatalogBaseUrlResolver resolver = new CatalogBaseUrlResolver(
                    fetcher,
                    ResourceString("RepoInfoUrl"),
                    ResourceString("CatalogRawUrlTemplate"),
                    Path.Combine(cacheDir, "repo-info.json"));
                CatalogService service = new CatalogService(
                    fetcher,
                    cacheDir,
                    resolver,
                    ResourceString("VersionsFileName"),
                    GetBasicCatalog);

                CatalogLoadOutcome outcome = service.LoadAsync(new Version(2, 1, 0), true).GetAwaiter().GetResult();

                Assert.That(outcome.UpdateRequired, Is.False, "App version must match the published catalog index");
                Assert.That(outcome.Catalog, Is.Not.Null);
                Assert.That(outcome.Catalog.BuildExtensionMap().Count, Is.GreaterThan(0));
            }
            finally
            {
                if (Directory.Exists(cacheDir))
                {
                    Directory.Delete(cacheDir, true);
                }
            }
        }
    }
}
