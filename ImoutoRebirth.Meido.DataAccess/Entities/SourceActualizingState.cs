using System;
using ImoutoRebirth.Meido.Core;

namespace ImoutoRebirth.Meido.DataAccess.Entities
{
    public class SourceActualizingStateEntity : EntityBase
    {
        public MetadataSource Source { get; set; }

        public int LastProcessedTagHistoryId { get; set; }

        public DateTimeOffset LastProcessedTagUpdateAt { get; set; }

        public DateTimeOffset LastProcessedNoteUpdateAt { get; set; }
    }
}