using ImoutoRebirth.Meido.Core.ParsingStatus;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Meido.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMeidoDomain(this IServiceCollection services)
        {
            services.AddTransient<IParsingService, ParsingService>();
            services.AddTransient<ISourceActualizerService, SourceActualizerService>();

            return services;
        }
    }
}