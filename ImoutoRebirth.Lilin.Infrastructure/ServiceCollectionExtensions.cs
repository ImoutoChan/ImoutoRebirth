using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLilinInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IFileTagRepository, FileTagRepository>();
            services.AddTransient<IFileNoteRepository, FileNoteRepository>();
            services.AddTransient<ITagTypeRepository, TagTypeRepository>();
            services.AddTransient<ITagRepository, TagRepository>();

            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<LilinDbContext>());

            return services;
        }
    }
}
