using NodaTime;

namespace ImoutoRebirth.Common.WebApi.NodaTime;

public class SchemaExamples
{
    public DateTimeZone DateTimeZone { get; set; }

    public Instant Instant { get; set; }

    public ZonedDateTime ZonedDateTime { get; set; }

    public Interval Interval { get; set; }

    public DateInterval DateInterval { get; set; }

    public Period Period { get; set; }

    public OffsetDate OffsetDate { get; set; }

    public OffsetTime OffsetTime { get; set; }

    public OffsetDateTime OffsetDateTime { get; set; }

    /// <summary>
    /// Creates example value by provided <see cref="DateTime"/> and <see cref="IDateTimeZoneProvider"/>.
    /// </summary>
    /// <param name="dateTimeZoneProvider">IDateTimeZoneProvider instance.</param>
    /// <param name="dateTimeUtc"><see cref="DateTime"/>. If not set then <see cref="DateTime.UtcNow"/> will be used.</param>
    /// <param name="dateTimeZone">Optional DateTimeZone name. If not set SystemDefault will be used.</param>
    public SchemaExamples(
        IDateTimeZoneProvider dateTimeZoneProvider,
        DateTime? dateTimeUtc = null,
        string? dateTimeZone = null)
    {
        var dateTimeUtcValue = dateTimeUtc ?? DateTime.UtcNow;

        if (dateTimeUtcValue.Kind != DateTimeKind.Utc)
            throw new ArgumentException("dateTimeUtc should be UTC", nameof(dateTimeUtc));

        DateTimeZone = dateTimeZone != null
            ? dateTimeZoneProvider.GetZoneOrNull(dateTimeZone) ?? dateTimeZoneProvider.GetSystemDefault()
            : dateTimeZoneProvider.GetSystemDefault();

        Instant = Instant.FromDateTimeUtc(dateTimeUtcValue);

        ZonedDateTime = Instant.InZone(DateTimeZone);

        Interval = new Interval(Instant,
            Instant.PlusTicks(TimeSpan.TicksPerDay)
                .PlusTicks(TimeSpan.TicksPerHour)
                .PlusTicks(TimeSpan.TicksPerMinute)
                .PlusTicks(TimeSpan.TicksPerSecond)
                .PlusTicks(TimeSpan.TicksPerMillisecond));

        DateInterval = new DateInterval(ZonedDateTime.Date, ZonedDateTime.Date.PlusDays(1));

        Period = Period.Between(
            ZonedDateTime.LocalDateTime,
            Interval.End.InZone(DateTimeZone).LocalDateTime,
            PeriodUnits.AllUnits);

        OffsetDate = new OffsetDate(ZonedDateTime.Date, ZonedDateTime.Offset);

        OffsetTime = new OffsetTime(ZonedDateTime.TimeOfDay, ZonedDateTime.Offset);

        OffsetDateTime = Instant.WithOffset(ZonedDateTime.Offset);
    }
}
