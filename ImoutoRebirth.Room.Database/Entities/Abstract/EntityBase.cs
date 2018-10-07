using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ImoutoRebirth.Room.Database.Entities.Abstract
{
    public class EntityBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }

        public DateTimeOffset AddedOn { get; set; }

        public DateTimeOffset ModifiedOn { get; set; }
    }
}