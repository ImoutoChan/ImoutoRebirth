using ImoutoRebirth.Common.EntityFrameworkCore;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.Database
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRoomDatabase(this IServiceCollection services)
        {
            services.AddTransient(typeof(SoftDeleteDbContextHelper<>));
            services.AddTransient<TimeTrackDbContextHelper>();

            return services;
        }
    }
}