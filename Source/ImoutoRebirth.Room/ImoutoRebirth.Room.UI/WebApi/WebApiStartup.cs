using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using ImoutoRebirth.Common.WebApi;
using ImoutoRebirth.Common.WebApi.NodaTime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Scalar.AspNetCore;

namespace ImoutoRebirth.Room.UI.WebApi;

public static class WebEndpointsExtensions
{
    public static IServiceCollection AddWebEndpoints(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(x => ConfigureDefaults(x.SerializerOptions));
        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(x => ConfigureDefaults(x.JsonSerializerOptions));

        services.AddMinimalSwagger(
            "ImoutoRebirth.Room WebApi Client",
            x => x.ConfigureForNodaTime(ConfigureDefaults(new JsonSerializerOptions())));
        services.AddOpenApi();
        
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
        services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
        
        services.AddRequestDecompression();
        return services;
    }
    
    public static WebApplication MapWebEndpoints(this WebApplication app)
    {
        app.UseRequestDecompression();
        app.UseResponseCompression();
        app.UseSwagger(options =>
        {
            options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
        });
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "ImoutoRebirth.Room API V1.0");
        });
        app.MapOpenApi();
        app.MapScalarApiReference();
        app.MapRootTo("swagger");

        app.MapCollectionsEndpoints();
        app.MapCollectionFilesEndpoints();
        app.MapDestinationFoldersEndpoints();
        app.MapSourceFoldersEndpoints();
        app.MapIntegrityReportsEndpoints();

        return app;
    }

    private static JsonSerializerOptions ConfigureDefaults(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter());
        options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
        options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        return options;
    }
}
