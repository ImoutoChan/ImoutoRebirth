using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack
{
    public class FillTimeTrackDbContextHelper
    {
        public void OnBeforeSaveChanges(ChangeTracker tracker)
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
    }
}