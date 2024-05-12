﻿using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ImoutoRebirth.Common.MassTransit;

public static class MassTransitSqlExtensions
{
    public static IServiceCollection AddSqlMassTransit(
        this IServiceCollection services,
        IConfiguration configuration,
        string consumingServiceName,
        Action<MassTransitConfigurator>? configure = null,   
        params Assembly[] addConsumersFromAssemblies)
    {
        var configurator = new MassTransitConfigurator();
        configure?.Invoke(configurator);
        
        // postgres transport
        var builder = new NpgsqlConnectionStringBuilder(configuration.GetConnectionString("MassTransit"));
        services.AddOptions<SqlTransportOptions>().Configure(options =>
        {
            options.Host = builder.Host;
            options.Database = builder.Database;
            options.Schema = "transport";
            options.Role = "transport";
            options.Username = builder.Username;
            options.Password = builder.Password;
            options.AdminUsername = builder.Username;
            options.AdminPassword = builder.Password;
        });
        services.AddPostgresMigrationHostedService();
        
        services.AddMassTransit(
            x =>
            {
                x.SetEndpointNameFormatter(new ImoutoRebirthEndpointNameFormatter(consumingServiceName));
                
                x.AddConfigureEndpointsCallback((_, _, cfg) =>
                {
                    cfg.PrefetchCount = 1;
                    cfg.UseMessageRetry(GetRetryPolicy);
                });

                if (addConsumersFromAssemblies.Any())
                {
                    // types should be public to register automatically
                    x.AddConsumers(addConsumersFromAssemblies);
                }
                
                x.UsingPostgres((context, cfg) =>
                {
                    cfg.UseDbMessageScheduler();
                    
                    configurator.ConfigureCustomEndpoint?.Invoke(context, cfg);
                    
                    cfg.ConfigureEndpoints(context);
                });
            });

        return services;
    }
    
    public static IServiceCollection AddMassTransitTestHarness(
        this IServiceCollection services,
        Action<MassTransitConfigurator>? configure = null)
    {
        var configurator = new MassTransitConfigurator();
        configure?.Invoke(configurator);

        services.AddMassTransitTestHarness(
            x =>
            {
                x.UsingInMemory((context, cfg) =>
                {
                    configurator.ConfigureCustomEndpoint?.Invoke(context, cfg);
                });
                x.AddTelemetryListener(true);
            });

        return services;
    }

    private static void GetRetryPolicy(IRetryConfigurator x) => x.Intervals(30, 60, 120);
}
