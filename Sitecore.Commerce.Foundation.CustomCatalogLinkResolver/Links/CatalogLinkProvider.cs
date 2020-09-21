using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Repositories;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Links;
using Sitecore.Resources.Media;
using System;
using System.Globalization;
using Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Extensions;

namespace Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Links
{
    public class CatalogLinkProvider : Sitecore.Commerce.XA.Foundation.Catalog.Providers.CatalogLinkProvider
    {
        protected ICatalogConfigurationRepository Repository { get; set; }

        public CatalogLinkProvider()
        {
            this.Repository = ServiceLocator.ServiceProvider.GetService<ICatalogConfigurationRepository>();
        }

        public override string GetItemUrl(Item item, UrlOptions options)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)item, nameof(item));
            Sitecore.Diagnostics.Assert.ArgumentNotNull((object)options, nameof(options));

            if (!this.StorefrontContext.IsStorefrontSite)
                return base.GetItemUrl(item, options);

            var itemType = this.GetItemType(item);

            if (itemType == Sitecore.Commerce.XA.Foundation.Common.Constants.ItemTypes.Category)
            {
                var categoryItemUrl = item.GetCommerceItemUrl();
                if (!string.IsNullOrWhiteSpace(categoryItemUrl))
                {
                    return categoryItemUrl;
                }
            }

            var catalogRootItem = this.Repository.GetCatalogRootItem(item);
            options.UseDisplayName = true;

            var rootitemurl = string.Empty;            

            if (itemType == Sitecore.Commerce.XA.Foundation.Common.Constants.ItemTypes.Category || itemType == Sitecore.Commerce.XA.Foundation.Common.Constants.ItemTypes.Product)
            {
                var catalogFolderItem = GetCatalogFolderItem(item);

                if (catalogFolderItem != null)
                {
                    var currentItemUrl = base.GetItemUrl(item, options);

                    options.LanguageEmbedding = LanguageEmbedding.Never;
                    options.AlwaysIncludeServerUrl = false;
                    var catalogFolderItemUrl = base.GetItemUrl(catalogFolderItem, options);

                    rootitemurl = catalogRootItem != null ? base.GetItemUrl(catalogRootItem, options) : string.Empty;
                    var resolvedUrl = currentItemUrl.Replace(catalogFolderItemUrl, "/" + rootitemurl.TrimStart('/'));

                    return resolvedUrl;
                }
            }

            return base.GetItemUrl(item, options);
        }

        private Item GetCatalogFolderItem(Item item)
        {
            if (item == null || item.Parent == null)
                return null;

            if (item.Parent.TemplateID.ToString().Equals(Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Constants.TemplateID.CommerceCatalog))
            {
                return item.Parent;
            }

            return GetCatalogFolderItem(item.Parent);
        }

        public override string GetDynamicUrl(Item item, LinkUrlOptions options)
        {
            if (this.StorefrontContext.IsStorefrontSite && this.IsCommerceCategoryItem(item))
            {
                var categoryItemUrl = item.GetCommerceItemUrl();
                if (!string.IsNullOrWhiteSpace(categoryItemUrl))
                {
                    return categoryItemUrl;
                }
            }
            return base.GetDynamicUrl(item, options);
        }

        private bool IsCommerceCategoryItem(Item item)
        {
            Sitecore.Commerce.XA.Foundation.Common.Constants.ItemTypes itemType = this.ItemTypeProvider.GetItemType(item);
            return itemType == Sitecore.Commerce.XA.Foundation.Common.Constants.ItemTypes.Category;
        }

        private Sitecore.Commerce.XA.Foundation.Common.Constants.ItemTypes GetItemType(Item item)
        {
            return this.ItemTypeProvider.GetItemType(item);
        }        
    }
}