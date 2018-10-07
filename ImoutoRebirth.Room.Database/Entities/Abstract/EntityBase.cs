using System;

namespace ImoutoRebirth.Room.Database.Entities.Abstract
{
    public class EntityBase
    {
        public long Id { get; set; }

        public DateTimeOffset AddedOn { get; set; }

        public DateTimeOffset ModifiedOn { get; set; }
    }
}