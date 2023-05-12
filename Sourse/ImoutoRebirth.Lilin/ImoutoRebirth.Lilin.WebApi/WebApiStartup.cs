using System.Text.Json.Serialization;
using ImoutoRebirth.Common.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ImoutoRebirth.Lilin.WebApi;

/// <remarks>
///     todo: maybe use two extension methods instead
/// </remarks>
public class WebApiStartup
{
    private IConfiguration Configuration { get; }

    public WebApiStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(
                options =>
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddSwagger("ImoutoRebirth.Lilin WebApi Client", typeof(WebApiStartup).Assembly);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
            
        app.UseRouting();

        app.UseEndpoints(endpoints => endpoints.MapControllers());

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "ImoutoRebirth.Lilin API V1.0");
        });
    }
}