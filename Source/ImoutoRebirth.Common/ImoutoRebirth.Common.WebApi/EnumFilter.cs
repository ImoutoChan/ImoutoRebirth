using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ImoutoRebirth.Common.WebApi;

public sealed class EnumFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schemaInput, SchemaFilterContext context)
    {
        var schema = (OpenApiSchema)schemaInput;

        if (!IsEnum(context.Type, out var enumName, out var enumType)) 
            return;

        var extension = new JsonObject
        {
            {"name", enumName},
            {"modelAsString", false}
        };

        var names = new JsonArray();
        foreach (var name in Enum.GetNames(enumType))
            names.Add(name);

        schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();

        schema.Extensions.Add("x-ms-enum", new JsonNodeExtension(extension));
        schema.Extensions.Add("x-enumFlags", new JsonNodeExtension(IsFlagsEnum(enumType)));
        schema.Extensions.Add("x-enumNames", new JsonNodeExtension(names));

        schema.Enum ??= new List<JsonNode>();
        schema.Enum.Clear();
        foreach (var name in Enum.GetNames(enumType))
            schema.Enum.Add(name);
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
