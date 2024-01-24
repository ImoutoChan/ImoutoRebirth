using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddTransient(typeof(SoftDeleteDbContextHelper<>));
        services.AddPostgresDbContext<RoomDbContext>(connectionString);
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<RoomDbContext>());

        return services;
    }
}
