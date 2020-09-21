using Sitecore.Data.Items;

namespace Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Repositories
{
    public interface ICatalogConfigurationRepository
    {
        Item GetCatalogRootItem(Item item);
    }
}