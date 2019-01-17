using System;
using MassTransit.RabbitMq.Extensions.Hosting.Configuration;

namespace ImoutoRebirth.Common.MassTransit
{
    public static class RabbitSettingsExtensions
    {
        public static MassTransitRabbitMqHostingOptions ToOptions(this RabbitSettings rabbitSettings)
            => new MassTransitRabbitMqHostingOptions
            {
                RabbitMqUri = new Uri(rabbitSettings.Url),
                RabbitMqUsername = rabbitSettings.Username,
                RabbitMqPassword = rabbitSettings.Password
            };
    }
}