using System.Text.RegularExpressions;
using Humanizer;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Common.MassTransit;

public static class MassTransitExtensions
{
    public static IServiceCollection AddTrueMassTransit(
        this IServiceCollection services,
        RabbitSettings settings,
        string connectionName,
        Action<ITrueMassTransitConfigurator>? configureAction = null)
    {
        services.AddMassTransit(
            x => x.AddBus(
                innerServices => Bus.Factory.CreateUsingRabbitMq(
                    cfg =>
                    {
                        cfg.Host(
                            new Uri(settings.Url),
                            connectionName,
                            hostConfigurator =>
                            {
                                hostConfigurator.Username(settings.Username);
                                hostConfigurator.Password(settings.Password);
                            });

                        configureAction?.Invoke(new TrueMassTransitConfigurator(cfg, innerServices));
                    })));

        return services;
    }

    public static ITrueMassTransitConfigurator AddConsumer<TConsumer, TMessage>(
        this ITrueMassTransitConfigurator configurator,
        string messageSourceAppName,
        Action<IRabbitMqReceiveEndpointConfigurator>? endpointConfigurator = null)
        where TConsumer : class, IConsumer<TMessage>
        where TMessage : class
    {
        configurator.RabbitMqBusFactoryConfigurator.ReceiveEndpoint(
            GetQueueName<TMessage>(messageSourceAppName),
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
        Action<RabbitMqSendEndpointConfigurator>? sendEndpointConfigurator = null)
        where TMessage : class
    {
        var queueName = GetQueueName<TMessage>(targetAppName);

        if (sendEndpointConfigurator != null)
        {
            var sendConfigurator = new RabbitMqSendEndpointConfigurator();
            sendEndpointConfigurator(sendConfigurator);
            queueName += sendConfigurator.GetUrlParams();
        }

        EndpointConvention.Map<TMessage>(new Uri($"queue:{queueName}"));

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

        static string GetSnailName(Type type)
        {
            var name = type.IsInterface && Regex.IsMatch(type.Name, "^I[A-Z]")
                ? type.Name.Substring(1) // type is interface and looks like ISomeInterface
                : type.Name;
            var namespaceSnail = type.Namespace?.Replace(".", "").Underscore();
            return $"{namespaceSnail}_{name.Underscore()}";
        }
    }

    private static void GetRetryPolicy(IRetryConfigurator retryConfigurator)
        => retryConfigurator.Intervals(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60));
}
