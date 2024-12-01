using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ImoutoRebirth.Common.WebApi.NodaTime;

public static class SwaggerGenOptionsExtensions
{
    public static void ConfigureForNodaTime(this SwaggerGenOptions config, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(config);

        var nodaTimeSchemaSettings = options.CreateNodaTimeSchemaSettings();
        config.ConfigureForNodaTime(nodaTimeSchemaSettings);
    }

    public static void ConfigureForNodaTime(this SwaggerGenOptions config, NodaTimeSchemaSettings nodaTimeSchemaSettings)
    {
        config.ParameterFilter<NamingPolicyParameterFilter>(nodaTimeSchemaSettings);

        var schemas = new SchemasFactory(nodaTimeSchemaSettings).CreateSchemas();

        config.MapType<Instant>        (schemas.Instant);
        config.MapType<LocalDate>      (schemas.LocalDate);
        config.MapType<LocalTime>      (schemas.LocalTime);
        config.MapType<LocalDateTime>  (schemas.LocalDateTime);
        config.MapType<OffsetDateTime> (schemas.OffsetDateTime);
        config.MapType<ZonedDateTime>  (schemas.ZonedDateTime);
        config.MapType<Interval>       (schemas.Interval);
        config.MapType<DateInterval>   (schemas.DateInterval);
        config.MapType<Offset>         (schemas.Offset);
        config.MapType<Period>         (schemas.Period);
        config.MapType<Duration>       (schemas.Duration);
        config.MapType<OffsetDate>     (schemas.OffsetDate);
        config.MapType<OffsetTime>     (schemas.OffsetTime);
        config.MapType<DateTimeZone>   (schemas.DateTimeZone);

        // Nullable structs
        config.MapType<Instant?>       (schemas.Instant);
        config.MapType<LocalDate?>     (schemas.LocalDate);
        config.MapType<LocalTime?>     (schemas.LocalTime);
        config.MapType<LocalDateTime?> (schemas.LocalDateTime);
        config.MapType<OffsetDateTime?>(schemas.OffsetDateTime);
        config.MapType<ZonedDateTime?> (schemas.ZonedDateTime);
        config.MapType<Interval?>      (schemas.Interval);
        config.MapType<Offset?>        (schemas.Offset);
        config.MapType<Duration?>      (schemas.Duration);
        config.MapType<OffsetDate?>    (schemas.OffsetDate);
        config.MapType<OffsetTime?>    (schemas.OffsetTime);
    }
}
