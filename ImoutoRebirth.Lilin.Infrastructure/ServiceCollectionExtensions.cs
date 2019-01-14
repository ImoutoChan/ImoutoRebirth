using ImoutoRebirth.Lilin.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLilinInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IFileTagRepository, FileTagRepository>();
            services.AddTransient<IFileNoteRepository, FileNoteRepository>();

            return services;
        }
    }
}
