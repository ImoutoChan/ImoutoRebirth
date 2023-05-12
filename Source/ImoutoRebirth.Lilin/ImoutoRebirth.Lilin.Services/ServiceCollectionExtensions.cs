using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services.ApplicationServices;
using ImoutoRebirth.Lilin.Services.CQRS.Commands;
using ImoutoRebirth.Lilin.Services.MessageCommandHandlers;
using ImoutoRebirth.Lilin.Services.Quartz;
using ImoutoRebirth.Meido.MessageContracts;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReceiverApp = ImoutoRebirth.Meido.MessageContracts.ReceiverApp;

namespace ImoutoRebirth.Lilin.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLilinServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<SaveMetadataCommand>());
        services.AddLoggingBehavior();
        services.AddTransactionBehavior();

        services.AddTransient<UpdateMetadataCommandConsumer>();
        services.AddTransient<IFileInfoService, FileInfoService>();

        services.AddQuartzJob<RecalculateTagsCountersJob, RecalculateTagsCountersJob.Description>();
        services.AddMemoryCache();

        services.Configure<RecalculateTagCountersSettings>(
            configuration.GetSection(nameof(RecalculateTagCountersSettings)));

        return services;
    }

    public static ITrueMassTransitConfigurator AddLilinServicesForRabbit(
        this ITrueMassTransitConfigurator builder)
    {
        builder.AddConsumer<UpdateMetadataCommandConsumer, IUpdateMetadataCommand>(
                Lilin.MessageContracts.ReceiverApp.Name)
            .AddFireAndForget<ISavedCommand>(ReceiverApp.Name);

        return builder;
    }
}
