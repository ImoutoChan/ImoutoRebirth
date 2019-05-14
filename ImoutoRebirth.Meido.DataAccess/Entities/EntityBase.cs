using System;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;

namespace ImoutoRebirth.Meido.DataAccess.Entities
{
    public class EntityBase : ITimeTrackableEntity
    {
        public DateTimeOffset AddedOn { get; set; }

        public DateTimeOffset ModifiedOn { get; set; }
    }
}
