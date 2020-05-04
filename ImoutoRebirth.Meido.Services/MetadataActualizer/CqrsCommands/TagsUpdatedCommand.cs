using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Meido.Services.MetadataActualizer.CqrsCommands
{
    internal class TagsUpdatedCommand : ICommand
    {
        public TagsUpdatedCommand(int sourceId, int[] postIds, int lastHistoryId)
        {
            SourceId = sourceId;
            PostIds = postIds;
            LastHistoryId = lastHistoryId;
        }

        public int SourceId { get; }

        public int[] PostIds { get; }

        public int LastHistoryId { get; }
    }
}