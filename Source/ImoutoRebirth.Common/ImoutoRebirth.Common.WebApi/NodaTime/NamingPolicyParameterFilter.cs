using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ImoutoRebirth.Common.WebApi.NodaTime;

internal class NamingPolicyParameterFilter : IParameterFilter
{
    private readonly NodaTimeSchemaSettings _nodaTimeSchemaSettings;

    public NamingPolicyParameterFilter(NodaTimeSchemaSettings nodaTimeSchemaSettings)
        => _nodaTimeSchemaSettings = nodaTimeSchemaSettings;

    public void Apply(IOpenApiParameter parameterInput, ParameterFilterContext context)
    {
        var parameter = (OpenApiParameter)parameterInput;
        parameter.Name = _nodaTimeSchemaSettings.ResolvePropertyName(parameter.Name ?? "");
    }
}
