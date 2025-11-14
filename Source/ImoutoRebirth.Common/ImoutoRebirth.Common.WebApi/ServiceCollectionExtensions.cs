using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ImoutoRebirth.Common.WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(
        this IServiceCollection services, 
        string swaggerTitle,
        Assembly? assemblyWithControllers = null,
        Action<SwaggerGenOptions>? configure = null)
    {
        assemblyWithControllers ??= Assembly.GetCallingAssembly();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.UseInlineDefinitionsForEnums();
            c.SchemaFilter<EnumFilter>();
            c.SchemaFilter<RequireValueTypePropertiesSchemaFilter>();

            c.CustomOperationIds(e =>
            {
                var actionDescriptor = (ControllerActionDescriptor)e.ActionDescriptor;
                return $"{actionDescriptor.ControllerName}_{actionDescriptor.ActionName}";
            });
            c.SwaggerDoc("v1.0", new OpenApiInfo
            {
                Title = swaggerTitle,
                Version = "v1.0"
            });
            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{assemblyWithControllers.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
            
            configure?.Invoke(c);
        });

        return services;
    }
    
    public static IServiceCollection AddMinimalSwagger(
        this IServiceCollection services,
        string swaggerTitle,
        Action<SwaggerGenOptions>? configure = null)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SchemaFilter<EnumFilter>();
            c.SchemaFilter<RequireValueTypePropertiesSchemaFilter>();
            c.SupportNonNullableReferenceTypes();
            c.NonNullableReferenceTypesAsRequired();

            c.SwaggerDoc("v1.0", new OpenApiInfo
            {
                Title = swaggerTitle,
                Version = "v1.0"
            });
            
            c.TagActionsBy(e =>
            {
                var controllerName = e.RelativePath?.Split("/").FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                return [controllerName?.ToUpperInvariant()[..1] + controllerName?[1..]];
            });
            
            c.CustomOperationIds(e =>
            {
                var name = e.ActionDescriptor.EndpointMetadata.OfType<RouteNameMetadata>().FirstOrDefault()?.RouteName;
                var controllerName = e.RelativePath?.Split("/").FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                var upperControllerName = controllerName?.ToUpperInvariant()[..1] + controllerName?[1..];
                
                return upperControllerName + '_' + name;
            });

            configure?.Invoke(c);
        });

        return services;
    }

    public static WebApplication MapRootTo(this WebApplication app, string redirectPath)
    {
        var normalizedPath = redirectPath.StartsWith('/') ? redirectPath : $"/{redirectPath}";

        app.MapGet("/", () => Results.Redirect(normalizedPath))
            .ExcludeFromDescription();

        return app;
    }
}
