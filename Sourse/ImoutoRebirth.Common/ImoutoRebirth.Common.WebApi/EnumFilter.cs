using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ImoutoRebirth.Common.WebApi;

public sealed class EnumFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!IsEnum(context.Type, out var enumName, out var enumType)) 
            return;

        var extension = new OpenApiObject
        {
            ["name"] = new OpenApiString(enumName),
            ["modelAsString"] = new OpenApiBoolean(false)
        };

        schema.AddExtension("x-ms-enum", extension);
        schema.Enum = Enum.GetNames(enumType).Select(x => new OpenApiString(x) as IOpenApiAny).ToList();
    }


    private static bool IsEnum(
        Type t,
        [NotNullWhen(true)] out string? enumName,
        [NotNullWhen(true)] out Type? trueEnumType)
    {
        if (t.IsEnum)
        {
            enumName = t.Name;
            trueEnumType = t;
            return true;
        }

        var nullableType = Nullable.GetUnderlyingType(t);
        enumName = nullableType?.Name;

        trueEnumType = nullableType;
        return nullableType is { IsEnum: true };
    }
}
