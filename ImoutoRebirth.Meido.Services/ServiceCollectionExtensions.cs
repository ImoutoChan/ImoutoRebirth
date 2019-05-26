using ImoutoProject.Common.Cqrs.Behaviors;
using ImoutoRebirth.Arachne.MessageContracts.Commands;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.Services.Consumers;
using ImoutoRebirth.Meido.Services.Cqrs.Commands;
using ImoutoRebirth.Meido.Services.MetadataRequest;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ImoutoProject.Common.Cqrs.Events;
using ReceiverApp = ImoutoRebirth.Arachne.MessageContracts.ReceiverApp;

namespace ImoutoRebirth.Meido.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMeidoServices(this IServiceCollection services)
        {
            services.AddMediatR(typeof(AddNewFileCommand));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
            services.AddTransient<NewFileCommandConsumer>();
            services.AddTransient<SearchCompleteCommandConsumer>();

            services.AddTransient<IMetadataRequesterProvider, MetadataRequesterProvider>();
            services.AddTransient<IMetadataRequester, YandereMetadataRequester>();
            services.AddTransient<IMetadataRequester, DanbooruMetadataRequester>();
            services.AddTransient<IMetadataRequester, SankakuMetadataRequester>();

            services.AddTransient<IEventPublisher, EventPublisher>();

            return services;
        }

        public static ITrueMassTransitConfigurator AddMeidoServicesForRabbit(
            this ITrueMassTransitConfigurator builder)
        {
            builder.AddConsumer<NewFileCommandConsumer, INewFileCommand>()
                   .AddConsumer<SearchCompleteCommandConsumer, ISearchCompleteCommand>()
                   .AddFireAndForget<IYandereSearchMetadataCommand>(ReceiverApp.Name)
                   .AddFireAndForget<IDanbooruSearchMetadataCommand>(ReceiverApp.Name)
                   .AddFireAndForget<ISankakuSearchMetadataCommand>(ReceiverApp.Name);

            return builder;
        }
    }
}