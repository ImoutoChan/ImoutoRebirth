namespace ImoutoRebirth.Meido.MessageContracts;

public interface ITagsUpdatedCommand
{
    int SourceId { get; }

    int[] PostIds { get; }

    int LastHistoryId { get; }
}