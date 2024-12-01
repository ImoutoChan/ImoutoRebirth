using NodaTime;

namespace ImoutoRebirth.Common.WebApi.NodaTime;

public class NodaTimeSchemaSettings
{
    public Func<string, string> ResolvePropertyName { get; }

    public Func<object, string> FormatToJson { get; }

    public IDateTimeZoneProvider DateTimeZoneProvider { get; }

    public bool ShouldGenerateExamples { get; }

    public SchemaExamples SchemaExamples { get; }

    /// <param name="resolvePropertyName">Function that resolves property name by proper naming strategy.</param>
    /// <param name="formatToJson">Function that formats object as json text.</param>
    /// <param name="shouldGenerateExamples">Should the example node be generated.</param>
    /// <param name="schemaExamples"><see cref="SchemaExamples"/> for schema example values.</param>
    /// <param name="dateTimeZoneProvider"><see cref="IDateTimeZoneProvider"/> configured in Startup.</param>
    public NodaTimeSchemaSettings(
        Func<string, string> resolvePropertyName,
        Func<object, string> formatToJson,
        bool shouldGenerateExamples,
        SchemaExamples? schemaExamples = null,
        IDateTimeZoneProvider? dateTimeZoneProvider = null)
    {
        ResolvePropertyName = resolvePropertyName;
        FormatToJson = formatToJson;

        DateTimeZoneProvider = dateTimeZoneProvider ?? DateTimeZoneProviders.Tzdb;

        ShouldGenerateExamples = shouldGenerateExamples;
        SchemaExamples = schemaExamples ?? new SchemaExamples(
            DateTimeZoneProvider,
            dateTimeUtc: null,
            dateTimeZone: null);
    }
}
