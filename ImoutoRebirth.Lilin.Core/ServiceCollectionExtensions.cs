using ImoutoRebirth.Lilin.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLilinCore(this IServiceCollection services)
        {
            services.AddTransient<IMetadataUpdateService, MetadataUpdateService>();

            return services;
        }
    }
}