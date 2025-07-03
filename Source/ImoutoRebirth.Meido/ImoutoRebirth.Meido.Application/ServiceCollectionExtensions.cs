using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace ImoutoRebirth.Meido.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMeidoApplication(this IServiceCollection services)
    {
        services.AddDefaultMediatR(x => x.RegisterServicesFromAssemblyContaining<MarkMetadataSavedCommand>());
        services.AddLoggingBehavior();
        services.AddTransactionBehavior();

        services.AddTransient<IClock>(_ => SystemClock.Instance);
            
        return services;
    }
}
