using System.Text.Json.Serialization;
using ImoutoRebirth.Common.Cqrs;
using ImoutoRebirth.Common.Host;
using ImoutoRebirth.Common.Logging;
using ImoutoRebirth.Common.OpenTelemetry;
using ImoutoRebirth.Common.WebApi;
using ImoutoRebirth.Kekkai.Application;
using ImoutoRebirth.Kekkai.Auth;
using ImoutoRebirth.Kekkai.Controllers;
using ImoutoRebirth.Lilin.WebApi.Client;
using ImoutoRebirth.Room.WebApi.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core;

const string servicePrefix = "KEKKAI_";

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

services.AddWindowsService();
builder.SetWorkingDirectory();
builder.UseEnvironmentFromEnvironmentVariable(servicePrefix);
builder.UseConfiguration(servicePrefix);
builder.ConfigureSerilog(
    (loggerBuilder, appConfiguration, environment)
        => loggerBuilder
            .WithoutDefaultLoggers()
            .WithConsole()
            .WithAllRollingFile()
            .WithInformationRollingFile()
            .WithOpenSearch(appConfiguration, environment));
services.Configure<KestrelServerOptions>(x => x.AddServerHeader = false);


services.AddLilinWebApiClients(configuration.GetRequiredValue<string>("LilinUrl"));
services.AddRoomWebApiClients(configuration.GetRequiredValue<string>("RoomUrl"));
        
services.AddMediatR(x => x.RegisterServicesFromAssemblyContaining<FilesStatusesQueryHandler>());
services.AddLoggingBehavior();

services.AddTransient<SimpleAuthMiddleware>();
        
services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

services.AddSwagger("Kekkai API", typeof(FileStatusController).Assembly, c => c.OperationFilter<AddAuthTokenOperationFilter>());
services.AddCors(options => options.AddPolicy("AllowAnyOrigin", x => x.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
services.AddOpenTelemetry(builder.Environment, builder.Configuration);

var app = builder.Build();
app.UseRouting();
app.UseCors("AllowAnyOrigin");

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Kekkai API"));

app.UseMiddleware<SimpleAuthMiddleware>();
app.MapControllers();
app.Run();
