using System;

namespace ImoutoRebirth.Lilin.DataAccess.Entities
{
    public class EntityBase
    {
        public DateTimeOffset AddedOn { get; set; }

        public DateTimeOffset ModifiedOn { get; set; }
    }
}