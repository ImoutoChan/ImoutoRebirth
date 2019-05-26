using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Meido.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMeidoDomain(this IServiceCollection services)
        {
            services.AddTransient<IParsingService, ParsingService>();

            return services;
        }
    }
}