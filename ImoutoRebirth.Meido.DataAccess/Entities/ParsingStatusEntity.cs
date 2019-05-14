using System;
using ImoutoRebirth.Meido.Core;

namespace ImoutoRebirth.Meido.DataAccess.Entities
{
    public class ParsingStatusEntity : EntityBase
    {
        public Guid FileId { get; set; }

        public string Md5 { get; set; }

        public MetadataSource Source { get; set; }

        public int? FileIdFromSource { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public Status Status { get; set; }

        public string ErrorMessage { get; set; }
    }
}