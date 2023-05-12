using ImoutoRebirth.Common.Cqrs.Abstract;

namespace ImoutoRebirth.Meido.Services.Cqrs.Commands;

internal class MarkMetadataSavedCommand : ICommand
{
    public MarkMetadataSavedCommand(Guid fileId, int sourceId)
    {
        FileId = fileId;
        SourceId = sourceId;
    }

    public Guid FileId { get; }

    public int SourceId { get; }
}