using System.Text.Json.Serialization;
using ImoutoRebirth.Common.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Lilin.WebApi;

public static class WebApiStartupExtensions
{
    public static IServiceCollection ConfigureWebApp(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options => {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        
        services.AddSwagger("ImoutoRebirth.Lilin WebApi Client", typeof(WebApiStartupExtensions).Assembly);

        return services;
    }
    
    public static WebApplication UseWebApp(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "ImoutoRebirth.Lilin API V1.0");
        });

        app.MapFilesEndpoints();
        app.MapTagsEndpoints();
        
        return app;
    }
}
