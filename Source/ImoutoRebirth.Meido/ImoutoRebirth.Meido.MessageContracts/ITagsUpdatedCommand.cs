namespace ImoutoRebirth.Meido.MessageContracts;

public interface ITagsUpdatedCommand
{
    int SourceId { get; }

    string[] PostIds { get; }

    int LastHistoryId { get; }
}
