using System.Text.Json.Serialization;
using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.WebApi;
using ImoutoRebirth.Kekkai.Application;
using ImoutoRebirth.Kekkai.Auth;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client;
using MediatR;
using GenericHost = Microsoft.Extensions.Hosting.Host;

namespace ImoutoRebirth.Kekkai;

internal static class Program
{
    private const string ServicePrefix = "KEKKAI_";

    private static async Task Main(string[] args)
    {
        await CreateHostBuilder(args)
            .Build()
            .RunAsync();
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public static IHostBuilder CreateHostBuilder(string[] args)
        => GenericHost.CreateDefaultBuilder(args)
            .UseWindowsService()
            .SetWorkingDirectory()
            .UseEnvironmentFromEnvironmentVariable(ServicePrefix)
            .UseConfiguration(ServicePrefix)
            .ConfigureSerilog(
                (loggerBuilder, appConfiguration)
                    => loggerBuilder
                        .WithoutDefaultLoggers()
                        .WithConsole()
                        .WithAllRollingFile()
                        .WithInformationRollingFile()
                        .PatchWithConfiguration(appConfiguration))
            .ConfigureWebHostDefaults(
                webHostBuilder
                    => webHostBuilder
                        .UseKestrel(options => options.AddServerHeader = false)
                        .UseStartup<WebApiStartup>());
}

public class WebApiStartup
{
    private IConfiguration Configuration { get; }

    public WebApiStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLilinWebApiClients(
            Configuration.GetValue<string>("LilinUrl") ?? throw new ArgumentException("Unable to retrieve LilinUrl"));
        services.AddRoomWebApiClients(
            Configuration.GetValue<string>("RoomUrl") ?? throw new ArgumentException("Unable to retrieve RoomUrl"));
        
        services.AddMediatR(typeof(FilesStatusesQueryHandler));
        services.AddLoggingBehavior();

        services.AddTransient<SimpleAuthMiddleware>();
        
        services
            .AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddSwagger("Kekkai API", typeof(WebApiStartup).Assembly, c =>
        {
            c.OperationFilter<AddAuthTokenOperationFilter>();
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
            
        app.UseRouting();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Kekkai API");
        });

        app.UseMiddleware<SimpleAuthMiddleware>();
        
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
