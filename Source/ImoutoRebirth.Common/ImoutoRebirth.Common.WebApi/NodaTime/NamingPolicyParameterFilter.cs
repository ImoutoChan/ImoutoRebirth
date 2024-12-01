using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ImoutoRebirth.Common.WebApi.NodaTime;

internal class NamingPolicyParameterFilter : IParameterFilter
{
    private readonly NodaTimeSchemaSettings _nodaTimeSchemaSettings;

    public NamingPolicyParameterFilter(NodaTimeSchemaSettings nodaTimeSchemaSettings)
        => _nodaTimeSchemaSettings = nodaTimeSchemaSettings;

    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        => parameter.Name = _nodaTimeSchemaSettings.ResolvePropertyName(parameter.Name);
}
