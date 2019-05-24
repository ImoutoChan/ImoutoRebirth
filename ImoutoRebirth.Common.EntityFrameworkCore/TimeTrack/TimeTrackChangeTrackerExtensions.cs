using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack
{
    public static class TimeTrackChangeTrackerExtensions
    {
        public static void TrackImplicitTimeBeforeSaveChanges(this ChangeTracker tracker)
        {
            foreach (var entityEntry in tracker.Entries<ITimeTrackableEntity>())
            {
                switch (entityEntry.State)
                {
                    case EntityState.Added:
                        entityEntry.Entity.AddedOn = DateTimeOffset.Now;
                        entityEntry.Entity.ModifiedOn = DateTimeOffset.Now;
                        break;
                    case EntityState.Modified:
                        entityEntry.Entity.ModifiedOn = DateTimeOffset.Now;
                        break;
                }
            }
        }

        public static void TrackShadowedTimeBeforeSaveChanges(this ChangeTracker tracker)
        {
            var trackableEntries = tracker
                                  .Entries()
                                  .Where(x => x.Metadata.FindProperty(TimeTrackingPropertyNames.ModifiedOn) != null);

            foreach (var entityEntry in trackableEntries)
            {
                switch (entityEntry.State)
                {
                    case EntityState.Added:
                    case EntityState.Modified:
                        entityEntry
                           .Property(TimeTrackingPropertyNames.ModifiedOn)
                           .CurrentValue = DateTimeOffset.Now;
                        break;
                }
            }
        }
    }
}