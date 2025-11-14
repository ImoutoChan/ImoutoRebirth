using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ImoutoRebirth.Kekkai.Auth;

public class AddAuthTokenOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // operation.Parameters ??= new List<OpenApiParameter>();

        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor _)
            return;

        operation.Parameters!.Add(new OpenApiParameter
        {
            Name = "token",
            In = ParameterLocation.Query,
            Description = "The auth token",
            Required = true
        });
    }
}
