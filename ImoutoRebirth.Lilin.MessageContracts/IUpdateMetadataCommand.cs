using System;

namespace ImoutoRebirth.Lilin.MessageContracts
{
    public interface IUpdateMetadataCommand
    {
        Guid FileId { get; }

        MetadataSource MetadataSource { get; }

        IFileNote[] FileNotes { get; }

        IFileTag[] FileTags { get; }
    }
}