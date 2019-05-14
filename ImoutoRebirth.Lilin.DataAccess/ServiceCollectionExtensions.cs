using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.DataAccess
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLilinDataAccess(this IServiceCollection services, string connectionString)
        {
            services.AddTransient<TimeTrackDbContextHelper>();
            services.AddDbContext<LilinDbContext>(o => o.UseNpgsql(connectionString));

            return services;
        }
    }
}