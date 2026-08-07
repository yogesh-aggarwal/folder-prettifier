using Newtonsoft.Json;
using System;

namespace FolderPrettifier
{
    public class CatalogLoadResult
    {
        public Catalog Catalog { get; set; }

        public bool UpdateRequired { get; set; }
    }

    public static class CatalogResolver
    {
        public static CatalogLoadResult Resolve(System.Version appVersion, string versionsJson, string selectedCatalogJson, string embeddedJson)
        {
            CatalogSelection selection = CatalogSelector.Select(versionsJson, appVersion);
            if (selection.Status == CatalogIndexStatus.NoMatch)
            {
                return new CatalogLoadResult { UpdateRequired = true };
            }

            Catalog catalog = null;
            if (selection.Status == CatalogIndexStatus.Selected)
            {
                catalog = ParseCompatibleCatalog(selectedCatalogJson, appVersion);
            }

            if (catalog == null)
            {
                catalog = ParseCompatibleCatalog(embeddedJson, appVersion);
            }

            return new CatalogLoadResult { Catalog = catalog };
        }

        private static Catalog ParseCompatibleCatalog(string catalogJson, System.Version appVersion)
        {
            if (string.IsNullOrEmpty(catalogJson)) return null;
            try
            {
                Catalog catalog = JsonConvert.DeserializeObject<Catalog>(catalogJson);
                if (catalog != null && !catalog.IsCompatibleWith(appVersion))
                {
                    return null;
                }
                return catalog;
            }
            catch
            {
                return null;
            }
        }
    }
}
