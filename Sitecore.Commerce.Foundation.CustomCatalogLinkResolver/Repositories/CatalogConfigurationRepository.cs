using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.XA.Foundation.Multisite;
using System;
using Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Extensions;

namespace Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Repositories
{
    public class CatalogConfigurationRepository : ICatalogConfigurationRepository
    {
        public ISiteInfoResolver siteInfoResolver { get; set; }

        public CatalogConfigurationRepository(ISiteInfoResolver SiteInfo)
        {
            this.siteInfoResolver = SiteInfo;
        }

        public Item GetCatalogRootItem(Item item = null)
        {
            var rootItem = (Item)null;

            if (item == null)
            {
                rootItem = Extensions.ItemExtensions.GetSiteRootItem();
            }
            else
            {
                rootItem = item.GetSiteRootItem(siteInfoResolver);
            }

            if (rootItem == null)
            {
                return null;
            }

            var settingsItem = rootItem.GetSettingsItem();

            if (settingsItem == null)
            {
                return null;
            }

            var rootItemField = settingsItem.Fields["Catalog Root Item"]; //TODO: need to create field on setting item

            if (rootItemField != null)
            {
                var rootItemFieldValue = rootItemField.Value;

                if (!string.IsNullOrEmpty(rootItemFieldValue))
                {                    
                    return rootItemFieldValue.GetItemById(Sitecore.Context.Database);
                }
            }

            return null;
        }

        //private Item GetSiteRootItem(Item item)
        //{
        //    var siteInfo = this.siteInfoResolver.GetSiteInfo(item);
        //    if (siteInfo != null)
        //    {
        //        var homeItemPath = string.Concat(siteInfo.RootPath, siteInfo.StartItem);

        //        return homeItemPath.GetItemByPath(item.Database);
        //    }
        //    return null;
        //}
    }
}