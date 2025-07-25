﻿using System.Reflection;
using ImoutoRebirth.Common.MassTransit;
using ImoutoRebirth.Room.Application.Services;
using ImoutoRebirth.Room.Database;
using ImoutoRebirth.Room.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Npgsql;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests.Fixtures;

[CollectionDefinition("WebApplication")]
public class WebApplicationCollection : ICollectionFixture<TestWebApplicationFactory<Program>>;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private const string TestDatabaseName = "RoomIntegrationTests";
    private const string ConnectionString = $"Server=localhost;Port=5432;Database={TestDatabaseName};User Id=postgres;Password=postgres;";
    private const string PostgresConnectionString = $"Server=localhost;Port=5432;Database=postgres;User Id=postgres;Password=postgres;";

    public HttpClient Client => CreateClient();

    public IServiceScope GetScope() => Services.CreateScope();

    public RoomDbContext GetDbContext(IServiceScope scope) => scope.ServiceProvider.GetRequiredService<RoomDbContext>();
    
    public string TestsTempLocation 
        => Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName, "temp");
    
    public string TestsLocation 
        => new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;

    public Mock<IWebhookUploader> WebhookUploaderMock { get; } = new();
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services
                
                // disable default context
                .Where(d => d.ServiceType == typeof(DbContextOptions<RoomDbContext>))
                .Union(services.Where(d => d.ServiceType == typeof(RoomDbContext)))
                
                // disable quartz 
                .Union(services.Where(
                    d => d.ServiceType == typeof(IHostedService)
                         && d.ImplementationType?.Name == "QuartzHostedService"))
                
            // replace IWebhookUploader with mock
            .Union(services.Where(x => x.ServiceType == typeof(IWebhookUploader)))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.AddDbContext<RoomDbContext>(x => x.UseNpgsql(ConnectionString, y => y.UseNodaTime()));
            services.AddMassTransitTestHarness(с => с.AddRoomMassTransitSetup());
            services.AddTransient<IWebhookUploader>(x => WebhookUploaderMock.Object);
        });

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<RoomDbContext>().Database.MigrateAsync();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        // delete test database
        using var connection = new NpgsqlConnection(PostgresConnectionString);
        connection.Open();

        const string terminateCommandText
            = $"""
               SELECT pg_terminate_backend(pg_stat_activity.pid)
               FROM pg_stat_activity
               WHERE pg_stat_activity.datname = '{TestDatabaseName}' AND pid <> pg_backend_pid();
               """;
        
        using var terminateCommand = new NpgsqlCommand(terminateCommandText, connection);
        terminateCommand.ExecuteNonQuery();

        using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{TestDatabaseName}\";", connection);
        command.ExecuteNonQuery();
        
        // clean temp folder
        if (Directory.Exists(TestsTempLocation))
            Directory.Delete(TestsTempLocation, true);
        
        base.Dispose(disposing);
    }
}
