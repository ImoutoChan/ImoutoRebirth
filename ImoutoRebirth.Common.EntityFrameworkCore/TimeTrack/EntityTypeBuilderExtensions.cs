using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack
{
    public static class EntityTypeBuilderExtensions
    {
        public static void AddShadowTimeTracking(this EntityTypeBuilder builder)
        {
            builder.Property<Instant>(TimeTrackingPropertyNames.AddedOn)
                   .IsRequired()
                   .HasValueGenerator<CurrentInstantValueGenerator>();

            builder.Property<Instant>(TimeTrackingPropertyNames.ModifiedOn)
                   .IsRequired();
        }
    }
}
