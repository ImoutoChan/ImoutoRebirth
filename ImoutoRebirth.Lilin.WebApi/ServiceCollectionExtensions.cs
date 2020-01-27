using System;
using System.IO;
using ImoutoRebirth.Lilin.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ImoutoRebirth.Lilin.WebApi
{
    public static class ServiceCollectionExtensions
    {
        /// <remarks>
        ///     todo: extract to common library
        /// </remarks>>
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.CustomOperationIds(e =>
                {
                    var actionDescriptor = (ControllerActionDescriptor)e.ActionDescriptor;
                    return $"{actionDescriptor.ControllerName}_{actionDescriptor.ActionName}";
                });
                c.SwaggerDoc("v1.0", new OpenApiInfo
                {
                    Title = "ImoutoRebirth.Lilin API",
                    Version = "v1.0"
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{typeof(FilesController).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            return services;
        }
    }
}