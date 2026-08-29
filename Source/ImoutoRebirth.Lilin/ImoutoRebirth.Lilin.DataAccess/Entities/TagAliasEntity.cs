using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using NodaTime;

namespace ImoutoRebirth.Lilin.DataAccess.Entities;

public class TagAliasEntity : ITimeTrackableEntity
{
    public Guid TagId { get; set; }

    public Guid AliasTagId { get; set; }

    public TagEntity? Tag { get; set; }

    public TagEntity? AliasTag { get; set; }

    public Instant AddedOn { get; set; }

    public Instant ModifiedOn { get; set; }
}
