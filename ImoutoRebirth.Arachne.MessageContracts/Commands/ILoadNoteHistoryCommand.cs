using System;
using ImoutoRebirth.Arachne.Core.Models;

namespace ImoutoRebirth.Arachne.MessageContracts.Commands
{
    public interface ILoadNoteHistoryCommand
    {
        SearchEngineType SearchEngineType { get; }

        DateTimeOffset LastProcessedNoteUpdateAt { get; }
    }
}