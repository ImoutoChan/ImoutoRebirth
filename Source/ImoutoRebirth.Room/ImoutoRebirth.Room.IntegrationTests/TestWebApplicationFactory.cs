using ImoutoRebirth.Room.Database;
using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Xunit;

namespace ImoutoRebirth.Room.IntegrationTests;

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
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<RoomDbContext>))
                .Union(services.Where(d => d.ServiceType == typeof(RoomDbContext)))
                .ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.AddDbContext<RoomDbContext>(x => x.UseNpgsql(ConnectionString, y => y.UseNodaTime()));
            services.AddMassTransitTestHarness();
        });

        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<RoomDbContext>().Database.MigrateAsync();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
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
        
        base.Dispose(disposing);
    }
}
