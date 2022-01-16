using System.ComponentModel.DataAnnotations.Schema;
using ImoutoRebirth.Common.EntityFrameworkCore.TimeTrack;

namespace ImoutoRebirth.Room.Database.Entities.Abstract;

public class EntityBase : ITimeTrackableEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public DateTimeOffset AddedOn { get; set; }

    public DateTimeOffset ModifiedOn { get; set; }
}