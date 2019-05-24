using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack
{
    public static class EntityTypeBuilderExtensions
    {
        public static void AddShadowTimeTracking(this EntityTypeBuilder builder)
        {
            builder.Property<DateTimeOffset>(TimeTrackingPropertyNames.AddedOn)
                   .IsRequired()
                   .HasValueGenerator<CurrentDateTimeOffsetValueGenerator>();

            builder.Property<DateTimeOffset>(TimeTrackingPropertyNames.ModifiedOn)
                   .IsRequired();
        }
    }
}