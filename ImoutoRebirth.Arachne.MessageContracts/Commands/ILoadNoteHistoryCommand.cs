using System;

namespace ImoutoRebirth.Arachne.MessageContracts.Commands
{
    public interface ILoadNoteHistoryCommand
    {
        SearchEngineType SearchEngineType { get; }

        DateTimeOffset LastProcessedNoteUpdateAt { get; }
    }
}