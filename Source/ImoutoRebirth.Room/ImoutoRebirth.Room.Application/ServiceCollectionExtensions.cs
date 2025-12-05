using System.Reflection;
using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Common.Jobs;
using ImoutoRebirth.Room.Application.Cqrs;
using ImoutoRebirth.Room.Application.Cqrs.IntegritySlice;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace ImoutoRebirth.Room.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomApplication(
        this IServiceCollection services,
        params Assembly[] registerAssemblies)
    {
        services.AddDefaultMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<OverseeCommandHandler>();
            x.RegisterServicesFromAssemblies(registerAssemblies);
        });
        services.AddLoggingBehavior();
        services.AddTransactionBehavior();

        services.AddTransient<IClock>(_ => SystemClock.Instance);

        services.AddTransient<IIntegrityReportJobRunner, IntegrityReportJobRunner>();
        services.AddRunningJobs<RoomJobType>();

        return services;
    }
}

public enum RoomJobType
{
    IntegrityReportBuilder
}
