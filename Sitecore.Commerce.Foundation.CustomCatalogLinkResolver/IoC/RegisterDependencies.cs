using Sitecore;
using Sitecore.Commerce.Engine.Connect.Pipelines.Carts;

namespace Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Ioc
{
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Foundation.CustomCatalogLinkResolver.Repositories;
    using Sitecore.DependencyInjection;

    public class RegisterDependencies : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<ICatalogConfigurationRepository, CatalogConfigurationRepository>();
        }
    }
}