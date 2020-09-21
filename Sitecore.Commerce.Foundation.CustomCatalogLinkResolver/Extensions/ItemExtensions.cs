using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.XA.Foundation.Multisite;
using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.Globalization;
using Sitecore.Resources.Media;
using System.Globalization;

namespace Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Extensions
{
    public static class ItemExtensions
    {
        public static Item GetSettingsItem(this Item contextItem)
        {
            return ServiceLocator.ServiceProvider.GetService<IMultisiteContext>().GetSettingsItem(contextItem);
        }

        public static Item GetSiteRootItem()
        {
            if (Sitecore.Context.Database != null || Sitecore.Context.Site != null)
            {
                return Sitecore.Context.Database.GetItem(Sitecore.Context.Site.StartPath);
            }

            return null;
        }

        public static Item GetSiteRootItem(this Item item, ISiteInfoResolver siteInfoResolver)
        {
            var siteInfo = siteInfoResolver.GetSiteInfo(item);
            if (siteInfo != null)
            {
                var homeItemPath = string.Concat(siteInfo.RootPath, siteInfo.StartItem);

                return homeItemPath.GetItemByPath(item.Database);
            }
            return null;
        }

        public static Item GetItemById(this string id, Database database = null, Language language = null)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                return Sitecore.Context.Database.GetItem(Sitecore.Data.ID.Parse(id));
            }
            return null;
        }

        public static Item GetItemByPath(this string contentPath, Database database = null, Language language = null)
        {
            if (!string.IsNullOrWhiteSpace(contentPath))
            {
                database = database ?? Sitecore.Context.Database;

                if (database != null)
                {
                    if (language == null) return database.GetItem(contentPath);
                    return database.GetItem(contentPath, language);
                }
            }
            return null;
        }

        public static Item GetItem(this Guid? id, string databaseName, Language language = null)
        {
            if (id.HasValue) return GetItem(id.Value, databaseName, language);

            return null;
        }

        public static string LinkFieldUrl(this Item item, ID fieldID)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (ID.IsNullOrEmpty(fieldID)) throw new ArgumentNullException(nameof(fieldID));
            var field = item.Fields[fieldID];

            if (field == null || !(FieldTypeManager.GetField(field) is LinkField)) return string.Empty;
            LinkField linkField = field;
            var result = string.Empty;
            switch (linkField.LinkType.ToLower())
            {
                case "internal":
                    // Use LinkMananger for internal links, if link is not empty
                    //result = linkField.TargetItem != null ? managers.Link.GetItemUrl(linkField.TargetItem) : string.Empty;
                    result = linkField.TargetItem != null ? Sitecore.Links.LinkManager.GetItemUrl(linkField.TargetItem) : string.Empty;
                    break;
                case "media":
                    // Use MediaManager for media links, if link is not empty
                    //result = linkField.TargetItem != null ? managers.Media.GetMediaUrl(linkField.TargetItem) : string.Empty;
                    result = linkField.TargetItem != null ? MediaManager.GetMediaUrl(linkField.TargetItem) : string.Empty;
                    break;
                case "external":
                    // Just return external links
                    result = linkField.Url;
                    break;
                case "anchor":
                    // Prefix anchor link with # if link if not empty
                    result = !string.IsNullOrEmpty(linkField.Anchor) ? "#" + linkField.Anchor : string.Empty;
                    break;
                case "mailto":
                    // Just return mailto link
                    result = linkField.Url;
                    break;
                case "javascript":
                    // Just return javascript
                    result = linkField.Url;
                    break;
                default:
                    // Just please the compiler, this
                    // condition will never be met
                    result = linkField.Url;
                    break;
            }

            if (!string.IsNullOrEmpty(result))
            {
                if (!string.IsNullOrEmpty(linkField.QueryString))
                {
                    var separator = result.Contains("?") ? "&" : "?";
                    result = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", result, separator, linkField.QueryString);
                }
            }

            return result;
        }

        public static string GetCommerceItemUrl(this Item item)
        {
            var linkField = item.Fields[Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Constants.FieldName.LinkField];
            if (linkField != null)
            {
                var categoryUrl = item.LinkFieldUrl(linkField.ID);
                if (!string.IsNullOrEmpty(categoryUrl))
                {
                    return categoryUrl;
                }
            }

            return string.Empty;
        }
    }
}