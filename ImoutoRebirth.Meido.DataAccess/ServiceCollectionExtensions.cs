using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Meido.DataAccess
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMeidoDataAccess(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<TimeTrackDbContextHelper>();
            services.AddDbContext<MeidoDbContext>(o => o.UseNpgsql(connectionString));

            return services;
        }
    }
}