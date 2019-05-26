using System;

namespace ImoutoRebirth.Meido.MessageContracts
{
    public interface ISavedCommand
    {
        Guid FileId { get; }

        int SourceId { get; }
    }
}