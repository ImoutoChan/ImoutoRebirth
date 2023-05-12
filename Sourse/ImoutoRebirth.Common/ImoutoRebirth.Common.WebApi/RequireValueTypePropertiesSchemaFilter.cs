using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ImoutoRebirth.Common.WebApi;

public sealed class RequireValueTypePropertiesSchemaFilter : ISchemaFilter
{
    /// <summary>
    /// Initializes a new <see cref="RequireValueTypePropertiesSchemaFilter"/>.
    /// </summary>
    public RequireValueTypePropertiesSchemaFilter()
    {
    }

    /// <summary>
    /// Adds non-nullable value type properties in a <see cref="Type"/> to the set of required properties
    /// for that type.
    /// </summary>
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        foreach (var property in context.Type.GetProperties())
        {
            var schemaPropertyName = GetMemberCamelCaseName(property);

            // This check ensures that properties that are not in the schema are not added as required.
            // This includes properties marked with [IgnoreDataMember] or [JsonIgnore]
            // (should not be present in schema or required).
            if (schema.Properties?.ContainsKey(schemaPropertyName) != true) 
                continue;


            // Value type properties are required,
            // except: Properties of type Nullable<T> are not required.
            var propertyType = property.PropertyType;
            if (!propertyType.IsValueType 
                || propertyType.IsConstructedGenericType 
                && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                continue;


            // Properties marked with [Required] are already required (don't require it again).
            var alreadyRequired = property.CustomAttributes.Any(x => x.AttributeType == typeof(RequiredAttribute));

            if (alreadyRequired)
                continue;

            // Make the value type member required
            if (schema.Required == null)
            {
                schema.Required = new HashSet<string>();
            }
            schema.Required.Add(schemaPropertyName);
        }
    }

    private static string GetMemberCamelCaseName(MemberInfo member)
    {
        var name = member.Name;

        return name[0].ToString().ToLowerInvariant() + name[1..];
    }
}