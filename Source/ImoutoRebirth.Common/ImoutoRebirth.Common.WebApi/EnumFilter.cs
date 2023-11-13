using System.Diagnostics.CodeAnalysis;
using System.Reflection;
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

        var names = new OpenApiArray();
        names.AddRange(Enum.GetNames(enumType).Select(x => new OpenApiString(x) as IOpenApiAny));

        schema.AddExtension("x-ms-enum", extension);
        
        schema.AddExtension("x-enumFlags", new OpenApiBoolean(IsFlagsEnum(enumType)));
        schema.AddExtension("x-enumNames", names);
        
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

    private static bool IsFlagsEnum(Type t)
    {
        if (t.IsEnum)
        {
            return t.GetCustomAttribute<FlagsAttribute>() != null;
        }

        var nullableType = Nullable.GetUnderlyingType(t);

        if (nullableType is { IsEnum: true })
        {
            return nullableType.GetCustomAttribute<FlagsAttribute>() != null;
        }

        return false;
    }
}
