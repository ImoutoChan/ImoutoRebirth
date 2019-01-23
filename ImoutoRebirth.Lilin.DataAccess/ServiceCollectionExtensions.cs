using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.DataAccess
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLilinDataAccess(this IServiceCollection services)
        {
            services.AddTransient(typeof(SoftDeleteDbContextHelper<>));
            services.AddTransient<TimeTrackDbContextHelper>();

            return services;
        }
       }
}