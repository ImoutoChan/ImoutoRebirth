namespace ImoutoRebirth.Meido.MessageContracts;

public interface INotesUpdatedCommand
{
    int SourceId { get; }

    string[] PostIds { get; }

    DateTimeOffset LastNoteUpdateDate { get; }
}
