using System;

namespace ImoutoRebirth.Meido.MessageContracts
{
    public interface INotesUpdatedCommand
    {
        int SourceId { get; }

        int[] PostIds { get; }

        DateTimeOffset LastNoteUpdateDate { get; }
    }
}