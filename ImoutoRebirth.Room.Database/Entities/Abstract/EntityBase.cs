using System.ComponentModel.DataAnnotations.Schema;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;
using NodaTime;

namespace ImoutoRebirth.Room.Database.Entities.Abstract;

public class EntityBase : ITimeTrackableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public Instant AddedOn { get; set; }

    public Instant ModifiedOn { get; set; }
}
