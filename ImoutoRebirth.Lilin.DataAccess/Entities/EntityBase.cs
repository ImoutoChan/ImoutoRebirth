using System.ComponentModel.DataAnnotations.Schema;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using NodaTime;

namespace ImoutoRebirth.Lilin.DataAccess.Entities;

public class EntityBase : ITimeTrackableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public Instant AddedOn { get; set; }

    public Instant ModifiedOn { get; set; }
}
