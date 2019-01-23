using System;

namespace ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack
{
    public interface ITimeTrackableEntity
    {
        DateTimeOffset AddedOn { get; set; }

        DateTimeOffset ModifiedOn { get; set; }
    }
}