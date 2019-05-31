using ImoutoProject.Common.Cqrs;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Common.Quartz.Extensions;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.Services.Consumers;
using ImoutoRebirth.Meido.Services.Cqrs.Commands;
using ImoutoRebirth.Meido.Services.MetadataActualizer;
using ImoutoRebirth.Meido.Services.MetadataRequest;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReceiverApp = ImoutoRebirth.Arachne.MessageContracts.ReceiverApp;

namespace ImoutoRebirth.Meido.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMeidoServices(this IServiceCollection services)
        {
            services.AddMediatR(typeof(AddNewFileCommand));
            services.AddLoggingBehavior();
            services.AddTransactionBehavior();

            services.AddTransient<NewFileCommandConsumer>();
            services.AddTransient<SearchCompleteCommandConsumer>();
            services.AddTransient<SavedCommandConsumer>();

            services.AddTransient<IMetadataRequesterProvider, MetadataRequesterProvider>();
            services.AddTransient<IMetadataRequester, YandereMetadataRequester>();
            services.AddTransient<IMetadataRequester, DanbooruMetadataRequester>();
            services.AddTransient<IMetadataRequester, SankakuMetadataRequester>();

            services.AddQuartzJob<MetaActualizerJob, MetaActualizerJob.Description>();

            return services;
        }

        public static IServiceCollection ConfigureMeidoServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<MetadataActualizerSettings>(
                configuration.GetSection(nameof(MetadataActualizerSettings)));

            return services;
        }

        public static ITrueMassTransitConfigurator AddMeidoServicesForRabbit(
            this ITrueMassTransitConfigurator builder)
        {
            builder.AddConsumer<NewFileCommandConsumer, INewFileCommand>()
                   .AddConsumer<SearchCompleteCommandConsumer, ISearchCompleteCommand>()
                   .AddConsumer<SavedCommandConsumer, ISavedCommand>()
                   .AddFireAndForget<IYandereSearchMetadataCommand>(ReceiverApp.Name)
                   .AddFireAndForget<IDanbooruSearchMetadataCommand>(ReceiverApp.Name)
                   .AddFireAndForget<ISankakuSearchMetadataCommand>(ReceiverApp.Name)
                   .AddFireAndForget<ILoadTagHistoryCommand>(ReceiverApp.Name)
                   .AddFireAndForget<ILoadNoteHistoryCommand>(ReceiverApp.Name);

            return builder;
        }
    }
}