using System;
using ImoutoProject.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands
{
    internal class NotesUpdatedCommand : ICommand
    {
        public NotesUpdatedCommand(int sourceId, int[] postIds, DateTimeOffset lastNoteUpdateDate)
        {
            SourceId = sourceId;
            PostIds = postIds;
            LastNoteUpdateDate = lastNoteUpdateDate;
        }

        public int SourceId { get; }

        public int[] PostIds { get; }

        public DateTimeOffset LastNoteUpdateDate { get; }
    }
}