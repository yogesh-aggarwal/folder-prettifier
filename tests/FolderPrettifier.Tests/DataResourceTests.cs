using FolderPrettifier;
using NUnit.Framework;
using System;
using System.IO;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class DataResourceTests
    {
        private static void AssertWellFormedAbsoluteUrl(string name, string url)
        {
            Assert.That(string.IsNullOrWhiteSpace(url), Is.False, name + " must not be empty.");
            Uri uri;
            Assert.That(Uri.TryCreate(url, UriKind.Absolute, out uri), Is.True,
                name + " must be a well-formed absolute URL: '" + url + "'");
            Assert.That(uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp, Is.True,
                name + " must use http(s): '" + url + "'");
        }

        [Test]
        public void Urls_AreWellFormedAbsoluteHttpUrls()
        {
            AssertWellFormedAbsoluteUrl("RepoInfoUrl", Data.RepoInfoUrl);
            AssertWellFormedAbsoluteUrl("CatalogRawUrlTemplate", Data.CatalogRawUrlTemplate);
            AssertWellFormedAbsoluteUrl("InternetCheckUrl", Data.InternetCheckUrl);
            AssertWellFormedAbsoluteUrl("ReleasesApiUrl", Data.ReleasesApiUrl);
            AssertWellFormedAbsoluteUrl("ReleasesPageUrl", Data.ReleasesPageUrl);
        }

        [Test]
        public void CatalogRawUrlTemplate_HasBranchPlaceholderAndTrailingSlash()
        {
            StringAssert.Contains("{0}", Data.CatalogRawUrlTemplate);
            Assert.That(Data.CatalogRawUrlTemplate.EndsWith("/"), Is.True);
        }

        [Test]
        public void CatalogRawUrlTemplate_MatchesLocalCacheLayout()
        {
            // The remote catalog URL layout and the local cache folder name must
            // stay in sync, or cached/remote fetches diverge.
            StringAssert.Contains("/assets/" + Data.CatalogCacheDir + "/", Data.CatalogRawUrlTemplate);
        }

        [Test]
        public void CacheLayout_MatchesOnDiskCatalogAssets()
        {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            string catalogsDir = null;
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "assets", Data.CatalogCacheDir);
                if (Directory.Exists(candidate))
                {
                    catalogsDir = candidate;
                    break;
                }
                dir = dir.Parent;
            }

            Assert.That(catalogsDir, Is.Not.Null,
                "assets/" + Data.CatalogCacheDir + " must exist in the repo.");
            Assert.That(File.Exists(Path.Combine(catalogsDir, Data.VersionsFileName)), Is.True,
                "Cache layout must match the on-disk catalog index file.");
        }
    }
}
