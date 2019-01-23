using System;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;

namespace ImoutoRebirth.Lilin.DataAccess.Entities
{
    public class EntityBase : ITimeTrackableEntity
    {
        public DateTimeOffset AddedOn { get; set; }

        public DateTimeOffset ModifiedOn { get; set; }
    }
}