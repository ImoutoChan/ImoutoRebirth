using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Meido.Core.ParsingStatus;
using ImoutoRebirth.Meido.Core.SourceActualizingState;
using ImoutoRebirth.Meido.DataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Meido.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMeidoInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IUnitOfWork>(provider => provider.GetRequiredService<MeidoDbContext>());
            services.AddScoped<IEventStorage, EventStorage>();

            services.AddTransient<IParsingStatusRepository, ParsingStatusRepository>();
            services.AddTransient<ISourceActualizingStateRepository, SourceActualizingStateRepository>();

            return services;
        }
    }
}