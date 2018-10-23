using System;
using GreenPipes;
using ImoutoRebirth.Arachne.MessageContracts;
using MassTransit.RabbitMq.Extensions.Hosting.Contracts;
using MassTransit.RabbitMq.Extensions.Hosting.Extensions;

namespace ImoutoRebirth.Arachne.Service
{
    public static class ServiceCollectionExtensions
    {
        public static IMassTransitRabbitMqHostingBuilder AddSearchMetadataCommandConsumer(
            this IMassTransitRabbitMqHostingBuilder builder)
        {
            return builder.ConsumeByConvention<SearchMetadataCommandConsumer, ISearchMetadataCommand>(
                r => r.Intervals(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60)));
        }
    }
}