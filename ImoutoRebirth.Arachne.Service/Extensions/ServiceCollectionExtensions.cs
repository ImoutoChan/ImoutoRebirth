using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Arachne.Service.Consumers;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Meido.MessageContracts;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Arachne.Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddArachneServices(this IServiceCollection services)
        {
            services.AddTransient<ISearchMetadataCommandHandler, SearchMetadataCommandHandler>();
            services.AddTransient<IMeidoReporter, MeidoReporter>();
            services.AddTransient<EverywhereSearchMetadataCommandConsumer>();
            services.AddTransient<YandereSearchMetadataCommandConsumer>();
            services.AddTransient<DanbooruSearchMetadataCommandConsumer>();
            services.AddTransient<SankakuSearchMetadataCommandConsumer>();
            services.AddTransient<LoadTagHistoryCommandConsumer>();
            services.AddTransient<LoadNoteHistoryCommandConsumer>();

            return services;
        }

        public static ITrueMassTransitConfigurator AddArachneServicesForRabbit(
            this ITrueMassTransitConfigurator builder)
            => builder.AddConsumer<EverywhereSearchMetadataCommandConsumer, IEverywhereSearchMetadataCommand>()
                      .AddConsumer<YandereSearchMetadataCommandConsumer, IYandereSearchMetadataCommand>()
                      .AddConsumer<DanbooruSearchMetadataCommandConsumer, IDanbooruSearchMetadataCommand>()
                      .AddConsumer<SankakuSearchMetadataCommandConsumer, ISankakuSearchMetadataCommand>()
                      .AddConsumer<LoadTagHistoryCommandConsumer, ILoadTagHistoryCommand>()
                      .AddConsumer<LoadNoteHistoryCommandConsumer, ILoadNoteHistoryCommand>()
                      .AddFireAndForget<IUpdateMetadataCommand>(Lilin.MessageContracts.ReceiverApp.Name)
                      .AddFireAndForget<ISearchCompleteCommand>(Meido.MessageContracts.ReceiverApp.Name);
    }
}