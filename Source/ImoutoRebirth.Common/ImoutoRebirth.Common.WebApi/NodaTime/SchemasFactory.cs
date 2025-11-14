using Microsoft.OpenApi;
using NodaTime;

namespace ImoutoRebirth.Common.WebApi.NodaTime;

public class SchemasFactory
{
    private readonly NodaTimeSchemaSettings _settings;

    public SchemasFactory(NodaTimeSchemaSettings settings) => _settings = settings;

    public Schemas CreateSchemas()
    {
        var examples = _settings.SchemaExamples;

        // https://xml2rfc.tools.ietf.org/public/rfc/html/rfc3339.html#anchor14
        return new Schemas
        {
            Instant = () => StringSchema(examples.Instant, "date-time"),
            LocalDate = () => StringSchema(examples.ZonedDateTime.Date, "date"),
            LocalTime = () => StringSchema(examples.ZonedDateTime.TimeOfDay),
            LocalDateTime = () => StringSchema(examples.ZonedDateTime.LocalDateTime),
            OffsetDateTime = () => StringSchema(examples.OffsetDateTime, "date-time"),
            ZonedDateTime = () => StringSchema(examples.ZonedDateTime),
            Interval = () => new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { ResolvePropertyName(nameof(Interval.Start)), StringSchema(examples.Interval.Start, "date-time") },
                    { ResolvePropertyName(nameof(Interval.End)), StringSchema(examples.Interval.End, "date-time") },
                },
            },
            DateInterval = () => new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>
                {
                    { ResolvePropertyName(nameof(DateInterval.Start)), StringSchema(examples.DateInterval.Start, "date") },
                    { ResolvePropertyName(nameof(DateInterval.End)), StringSchema(examples.DateInterval.End, "date") },
                },
            },
            Offset = () => StringSchema(examples.ZonedDateTime.Offset),
            Period = () => StringSchema(examples.Period),
            Duration = () => StringSchema(examples.Interval.Duration),
            OffsetDate = () => StringSchema(examples.OffsetDate),
            OffsetTime = () => StringSchema(examples.OffsetTime),
            DateTimeZone = () => StringSchema(examples.DateTimeZone),
        };
    }

    private OpenApiSchema StringSchema(object exampleObject, string? format = null)
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            Example = _settings.ShouldGenerateExamples
                ? FormatToJson(exampleObject)
                : null,
            Format = format
        };
    }

    private string ResolvePropertyName(string propertyName) => _settings.ResolvePropertyName(propertyName);

    private string FormatToJson(object value) => _settings.FormatToJson(value);
}
