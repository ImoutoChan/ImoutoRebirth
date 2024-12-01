using Microsoft.OpenApi.Models;

namespace ImoutoRebirth.Common.WebApi.NodaTime;

public class Schemas
{
    public required Func<OpenApiSchema> Instant { get; set; }

    public required Func<OpenApiSchema> LocalDate { get; set; }

    public required Func<OpenApiSchema> LocalTime { get; set; }

    public required Func<OpenApiSchema> LocalDateTime { get; set; }

    public required Func<OpenApiSchema> OffsetDateTime { get; set; }

    public required Func<OpenApiSchema> ZonedDateTime { get; set; }

    public required Func<OpenApiSchema> Interval { get; set; }

    public required Func<OpenApiSchema> DateInterval { get; set; }

    public required Func<OpenApiSchema> Offset { get; set; }

    public required Func<OpenApiSchema> Period { get; set; }

    public required Func<OpenApiSchema> Duration { get; set; }

    public required Func<OpenApiSchema> OffsetDate { get; set; }

    public required Func<OpenApiSchema> OffsetTime { get; set; }

    public required Func<OpenApiSchema> DateTimeZone { get; set; }
}
