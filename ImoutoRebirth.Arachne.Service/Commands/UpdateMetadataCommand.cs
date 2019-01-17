using System;
using ImoutoRebirth.Lilin.MessageContracts;

namespace ImoutoRebirth.Arachne.Service.Commands
{
    public class UpdateMetadataCommand : IUpdateMetadataCommand
    {
        public Guid FileId { get; set; }

        public MetadataSource MetadataSource { get; set; }

        public IFileNote[] FileNotes { get; set; }

        public IFileTag[] FileTags { get; set; }
    }
}