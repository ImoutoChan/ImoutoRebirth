using System;
using NodaTime;

namespace ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack
{
    public interface ITimeTrackableEntity
    {
        Instant AddedOn { get; set; }

        Instant ModifiedOn { get; set; }
    }
}
