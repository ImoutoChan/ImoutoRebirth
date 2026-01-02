using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Lilin.UI.Quartz;
using ImoutoRebirth.Meido.MessageContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.UI;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLilinUi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddQuartzJob<RecalculateTagsCountersJob, RecalculateTagsCountersJob.Description>();
        services.AddQuartzJob<DeleteEmptyLocationTagsJob, DeleteEmptyLocationTagsJob.Description>();
        services.AddMemoryCache();

        services.Configure<RecalculateTagCountersSettings>(
            configuration.GetSection(nameof(RecalculateTagCountersSettings)));

        services.Configure<DeleteEmptyLocationTagsSettings>(
            configuration.GetSection(nameof(DeleteEmptyLocationTagsSettings)));

        return services;
    }

    public static MassTransitConfigurator AddLilinMassTransitSetup(this MassTransitConfigurator builder) 
        => builder.AddCommand<SavedCommand>();
}
