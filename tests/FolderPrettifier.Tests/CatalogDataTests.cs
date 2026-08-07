using FolderPrettifier;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class CatalogDataTests
    {
        private static string FindRepoRoot()
        {
            DirectoryInfo dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "assets", "catalogs", "versions.jsonc")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            throw new InvalidOperationException("Could not locate repo root (assets/catalogs/versions.jsonc not found).");
        }

        private static string CatalogsDir
        {
            get { return Path.Combine(FindRepoRoot(), "assets", "catalogs"); }
        }

        private static Version GetAppVersion()
        {
            string assemblyInfo = File.ReadAllText(Path.Combine(FindRepoRoot(), "src", "Properties", "AssemblyInfo.cs"));
            Match match = Regex.Match(assemblyInfo, @"AssemblyVersion\(""([\d.]+)""\)");
            if (!match.Success)
            {
                throw new InvalidOperationException("AssemblyVersion not found in AssemblyInfo.cs.");
            }
            return Version.Parse(match.Groups[1].Value);
        }

        private static string IndexJson
        {
            get { return File.ReadAllText(Path.Combine(CatalogsDir, "versions.jsonc")); }
        }

        private static Catalog LoadSelectedCatalog()
        {
            CatalogSelection selection = CatalogSelector.Select(IndexJson, GetAppVersion());
            Assert.That(selection.Status, Is.EqualTo(CatalogIndexStatus.Selected),
                "versions.jsonc must contain an entry compatible with app version " + GetAppVersion());
            string path = Path.Combine(CatalogsDir, selection.FileName);
            Assert.That(File.Exists(path), Is.True, "versions.jsonc points to missing file: " + selection.FileName);
            return JsonConvert.DeserializeObject<Catalog>(File.ReadAllText(path));
        }

        [Test]
        public void VersionsIndex_ParsesAndSelectsCatalogForCurrentAppVersion()
        {
            Dictionary<string, string> index = CatalogSelector.ParseIndex(IndexJson);

            Assert.That(index, Is.Not.Null, "versions.jsonc must parse.");
            Assert.That(index.Count, Is.GreaterThan(0), "versions.jsonc must map at least one version.");
        }

        [Test]
        public void VersionsIndex_AllKeysAreVersionsAndFilesExist()
        {
            Dictionary<string, string> index = CatalogSelector.ParseIndex(IndexJson);

            foreach (KeyValuePair<string, string> entry in index)
            {
                Version entryVersion;
                Assert.That(Version.TryParse(entry.Key, out entryVersion), Is.True,
                    "versions.jsonc key is not a version: " + entry.Key);
                Assert.That(File.Exists(Path.Combine(CatalogsDir, entry.Value)), Is.True,
                    "versions.jsonc points to missing file: " + entry.Value);
            }
        }

        [Test]
        public void SelectedCatalog_ParsesAndIsCompatibleWithAppVersion()
        {
            Catalog catalog = LoadSelectedCatalog();

            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.Version, Is.GreaterThan(0));
            Assert.That(catalog.IsCompatibleWith(GetAppVersion()), Is.True,
                "Selected catalog min-app-version must be <= app version " + GetAppVersion());
        }

        [Test]
        public void SelectedCatalog_HasWellFormedCategories()
        {
            Catalog catalog = LoadSelectedCatalog();

            Assert.That(catalog.Categories, Is.Not.Null.And.Not.Empty, "Catalog must define categories.");
            List<string> folders = new List<string>();
            foreach (CatalogCategory category in catalog.Categories)
            {
                Assert.That(string.IsNullOrWhiteSpace(category.Name), Is.False, "Category has no name.");
                Assert.That(string.IsNullOrWhiteSpace(category.Folder), Is.False, "Category '" + category.Name + "' has no folder.");
                Assert.That(category.Extensions, Is.Not.Null.And.Not.Empty,
                    "Category '" + category.Name + "' has no extensions.");
                Assert.That(category.Extensions.All(e => !string.IsNullOrWhiteSpace(e)), Is.True,
                    "Category '" + category.Name + "' contains empty extension entries.");
                folders.Add(category.Folder.ToLowerInvariant());
            }

            Assert.That(folders.Distinct().Count(), Is.EqualTo(folders.Count),
                "Multiple categories map to the same folder.");
        }

        [Test]
        public void SelectedCatalog_BuildsNonEmptyExtensionMapWithDefaultFolder()
        {
            Catalog catalog = LoadSelectedCatalog();

            Dictionary<string, string> map = catalog.BuildExtensionMap();
            Assert.That(map.Count, Is.GreaterThan(0), "Catalog must map at least one extension.");
            Assert.That(map.Values.All(v => !string.IsNullOrEmpty(v)), Is.True);
            Assert.That(string.IsNullOrWhiteSpace(catalog.DefaultFolder), Is.False);
            Assert.That(FileCategorizer.CategoryFor(Path.Combine("x", "unknown.ext"), map, catalog.DefaultFolder),
                Is.EqualTo(catalog.DefaultFolder));
        }

        [Test]
        public void SelectedCatalog_NoDuplicateExtensionsAcrossCategories()
        {
            Catalog catalog = LoadSelectedCatalog();

            int total = catalog.Categories.Sum(c => c.Extensions.Count);
            Dictionary<string, string> map = catalog.BuildExtensionMap();

            Assert.That(map.Count, Is.EqualTo(total),
                "Extensions listed under multiple categories would be silently overwritten (case-insensitive).");
        }
    }
}
