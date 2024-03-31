using System.IO.Compression;
using System.Text.Json.Serialization;
using ImoutoRebirth.Common.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

namespace ImoutoRebirth.Room.UI.WebApi;

public static class WebEndpointsExtensions
{
    public static IServiceCollection AddWebEndpoints(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(x =>
            x.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(x =>
            x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        
        services.AddMinimalSwagger("ImoutoRebirth.Room WebApi Client");
        
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
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "ImoutoRebirth.Room API V1.0");
        });

        app.MapCollectionsEndpoints();
        app.MapCollectionFilesEndpoints();
        app.MapDestinationFoldersEndpoints();
        app.MapSourceFoldersEndpoints();
        app.MapImoutoPicsUploaderEnabled();
        
        return app;
    }
}
