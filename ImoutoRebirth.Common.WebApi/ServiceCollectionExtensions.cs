using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ImoutoRebirth.Common.WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(
        this IServiceCollection services, 
        string swaggerTitle,
        Assembly? assemblyWithControllers = null)
    {
        assemblyWithControllers ??= Assembly.GetCallingAssembly();

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
        });

        return services;
    }
}