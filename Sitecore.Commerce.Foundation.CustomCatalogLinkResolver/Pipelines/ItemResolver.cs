using Microsoft.Extensions.DependencyInjection;
using Sitecore.Commerce.XA.Foundation.Common.Providers;
using Sitecore.Data.ItemResolvers;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;
using System;
using System.Linq;
using Sitecore.Data.Items;
using Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Repositories;

namespace Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Pipelines
{
    public class ItemResolver : HttpRequestProcessor
    {
        protected ICatalogConfigurationRepository Repository { get; set; }

        public ItemResolver()
        {
            this.Repository = ServiceLocator.ServiceProvider.GetService<ICatalogConfigurationRepository>();
        }

        public override void Process(HttpRequestArgs args)
        {
            try
            {
                Assert.ArgumentNotNull(args, "args");

                if(args.LocalPath.ToLower() == "/keepalive")
                {
                    return;
                }

                if (Sitecore.Context.Item != null
                    || Sitecore.Context.Database == null
                    || args.Url.ItemPath.Length == 0
                    || Sitecore.Context.Site == null)
                {
                    return;
                }

                string localUrl = args.LocalPath.ToLower();

                var catalogRootItem = this.Repository.GetCatalogRootItem(null);

                if (catalogRootItem == null)
                {
                    return;
                }

                var catalogRootItemUrl = string.Concat("/", catalogRootItem.Name);

                if (localUrl.Contains(catalogRootItemUrl.ToLower()))
                {
                    var rootItem = GetSiteRootItem();

                    //var catalogFolderItem = rootItem.GetChildren().FirstOrDefault(x => x.TemplateID.ToString().Equals(Constants.TemplateId.CommerceCatalogFolder));
                    var catalogFolderItem = rootItem.GetChildren().FirstOrDefault(x => x.TemplateID.ToString().Equals("{334E2B54-F913-411D-B159-A7B16D65242C}"));

                    var catalogItem = catalogFolderItem.Children.FirstOrDefault();

                    var resolvedName = catalogFolderItem.Name + "/" + catalogItem.Name;

                    var resolvedItemPath = localUrl.Replace(catalogRootItemUrl.ToLower(), resolvedName.ToLower());

                    var itempathresolver = ServiceLocator.ServiceProvider.GetRequiredService<ItemPathResolver>();

                    var resolvedItem = itempathresolver.ResolveItem(resolvedItemPath, rootItem);                    

                    Sitecore.Context.Item = resolvedItem;
                }
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("Error in Catalog item resolver", ex, this);
                return;
            }
        }
        
        public Item GetSiteRootItem()
        {
            if (Sitecore.Context.Database != null || Sitecore.Context.Site != null)
            {
                return Sitecore.Context.Database.GetItem(Sitecore.Context.Site.StartPath);
            }

            return null;
        }
    }
}