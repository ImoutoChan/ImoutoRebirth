using System.IO.Compression;
using System.Text.Json.Serialization;
using ImoutoRebirth.Common.WebApi;
using ImoutoRebirth.Room.WebApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Room.WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomWebApi(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddApplicationPart(typeof(CollectionsController).Assembly)
            .AddJsonOptions(
                options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddSwagger("ImoutoRebirth.Room WebApi Client", typeof(CollectionsController).Assembly);
        
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

    public static IApplicationBuilder ConfigureWebApi(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRequestDecompression();
        app.UseResponseCompression();
        
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseEndpoints(builder => builder.MapControllers());

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "ImoutoRebirth.Room API V1.0");
        });

        return app;
    }
}
