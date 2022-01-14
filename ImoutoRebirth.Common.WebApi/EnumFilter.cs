using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ImoutoRebirth.Common.WebApi;

public sealed class EnumFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!IsEnum(context.Type, out var enumName)) 
            return;

        var extension = new OpenApiObject
        {
            ["name"] = new OpenApiString(enumName ?? context.Type.Name),
            ["modelAsString"] = new OpenApiBoolean(false)
        };

        schema.AddExtension("x-ms-enum", extension);
        schema.Enum = Enum.GetNames(context.Type).Select(x => new OpenApiString(x) as IOpenApiAny).ToList();
    }

    private static bool IsEnum(Type t, out string? enumName)
    {
        if (t.IsEnum)
        {
            enumName = t.Name;
            return true;
        }

        var nullableType = Nullable.GetUnderlyingType(t);
        enumName = nullableType?.Name;

        return nullableType != null && nullableType.IsEnum;
    }
}