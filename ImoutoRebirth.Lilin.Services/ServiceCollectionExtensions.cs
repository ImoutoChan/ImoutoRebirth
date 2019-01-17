﻿using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services.MessageCommandHandlers;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLilinServices(this IServiceCollection services)
        {
            services.AddMediatR();

            return services;
        }

        public static IMassTransitRabbitMqHostingBuilder AddLilinServicesForRabbit(
            this IMassTransitRabbitMqHostingBuilder builder)
            => builder.AddDefaultConsumer<UpdateMetadataCommandConsumer, IUpdateMetadataCommand>();
    }
}