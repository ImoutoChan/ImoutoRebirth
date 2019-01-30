using System;
using System.IO;
using System.Text.RegularExpressions;
using GreenPipes;
using GreenPipes.Configurators;
using Humanizer;
using MassTransit;
using MassTransit.AspNetCoreIntegration;
using MassTransit.RabbitMqTransport;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Common.MassTransit
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddTrueMassTransit(
            this IServiceCollection services,
            RabbitSettings settings,
            string appName,
            Action<ITrueMassTransitConfigurator> configureAction = null)
        {
            services.AddMassTransit(
                provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    var host = cfg.Host(new Uri(settings.Url), appName, x =>
                    {
                        x.Username(settings.Username);
                        x.Password(settings.Password);
                    });

                    configureAction?.Invoke(new TrueMassTransitConfigurator(appName, cfg, provider, host));
                }));

            return services;
        }

        public static ITrueMassTransitConfigurator AddConsumer<TConsumer, TMessage>(
            this ITrueMassTransitConfigurator configurator,
            Action<IRabbitMqReceiveEndpointConfigurator> endpointConfigurator = null)
            where TConsumer : class, IConsumer<TMessage>
            where TMessage : class
        {
            configurator.RabbitMqBusFactoryConfigurator.ReceiveEndpoint(
                configurator.RabbitMqHost,
                GetQueueName<TMessage>(configurator.ApplicationName),
                x =>
                {
                    x.PrefetchCount = 1;
                    x.UseMessageRetry(GetRetryPolicy);
                    endpointConfigurator?.Invoke(x);

                    x.Consumer<TConsumer>(configurator.ServiceProvider);
                });

            return configurator;
        }

        public static ITrueMassTransitConfigurator AddFireAndForget<TMessage>(
            this ITrueMassTransitConfigurator configurator,
            string targetAppName,
            Action<IRabbitMqReceiveEndpointConfigurator> endpointConfigurator = null)
            where TMessage : class
        {
            var queueName = GetQueueName<TMessage>(targetAppName);

            var path = Path.Combine(configurator.RabbitMqHost.Address.ToString(), queueName);
            EndpointConvention.Map<TMessage>(new Uri(path));

            return configurator;
        }

        private static string GetQueueName<TMessage>(string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName))
            {
                throw new ArgumentNullException(nameof(applicationName));
            }

            var snailName = GetSnailName(typeof(TMessage));
            return $"{applicationName}_{snailName.Underscore()}";

            string GetSnailName(Type type)
            {
                var name = type.IsInterface && Regex.IsMatch(type.Name, "^I[A-Z]")
                    ? type.Name.Substring(1) // type is interface and looks like ISomeInterface
                    : type.Name;
                var namespaceSnail = type.Namespace.Replace(".", "").Underscore();
                return $"{namespaceSnail}_{name.Underscore()}";
            }
        }

        private static void GetRetryPolicy(IRetryConfigurator retryConfigurator)
            => retryConfigurator.Intervals(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60));
    }
}
