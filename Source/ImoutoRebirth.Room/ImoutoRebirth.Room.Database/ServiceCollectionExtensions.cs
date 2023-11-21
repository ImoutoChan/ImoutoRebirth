using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient(typeof(SoftDeleteDbContextHelper<>));

        services.AddDbContext<RoomDbContext>(builder
            => builder.UseNpgsql(
                configuration.GetConnectionString("RoomDatabase"), 
                x => x.UseNodaTime()));

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<RoomDbContext>());

        return services;
    }
}
