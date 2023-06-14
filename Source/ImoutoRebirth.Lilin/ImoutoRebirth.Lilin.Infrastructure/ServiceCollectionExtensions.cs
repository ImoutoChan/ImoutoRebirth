using ImoutoRebirth.Common.Domain;
using ImoutoRebirth.Common.Infrastructure;
using ImoutoRebirth.Lilin.Application.Persistence;
using ImoutoRebirth.Lilin.DataAccess;
using ImoutoRebirth.Lilin.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLilinInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IFileTagRepository, FileTagRepository>();
        services.AddTransient<IFileNoteRepository, FileNoteRepository>();
        services.AddTransient<ITagTypeRepository, TagTypeRepository>();
        services.AddTransient<ITagRepository, TagRepository>();
        services.AddTransient<IFileInfoRepository, FileInfoRepository>();

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<LilinDbContext>());
        services.AddScoped<IEventStorage, EventStorage>();

        services.AddDistributedBus();

        return services;
    }
}
