using ImoutoProject.Common.Cqrs.Behaviors;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.Services.Consumers;
using ImoutoRebirth.Meido.Services.Cqrs.Commands;
using ImoutoRebirth.Meido.Services.MetadataRequest;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddTransient<IMetadataRequesterProvider, MetadataRequesterProvider>();
            services.AddTransient<IMetadataRequester, YandereMetadataRequester>();
            services.AddTransient<IMetadataRequester, DanbooruMetadataRequester>();
            services.AddTransient<IMetadataRequester, SankakuMetadataRequester>();

            return services;
        }

        public static ITrueMassTransitConfigurator AddMeidoServicesForRabbit(
            this ITrueMassTransitConfigurator builder)
        {
            builder.AddConsumer<NewFileCommandConsumer, INewFileCommand>();

            return builder;
        }
    }
}