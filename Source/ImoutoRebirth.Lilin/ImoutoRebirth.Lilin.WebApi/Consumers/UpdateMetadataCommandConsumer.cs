using ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;
using ImoutoRebirth.Lilin.MessageContracts;
using MassTransit;
using MassTransit.Mediator;

namespace ImoutoRebirth.Lilin.WebApi.Consumers;

internal class UpdateMetadataCommandConsumer : IConsumer<IUpdateMetadataCommand>
{
    private readonly IMediator _mediator;

    public UpdateMetadataCommandConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<IUpdateMetadataCommand> context)
    {
        var message = context.Message;

        var command = new ActualizeFileInfoForSourceCommand(
            message.FileId,
            Convert(message.MetadataSource),
            
            message.FileTags
                .Select(x => new ActualizeTag(x.Type, x.Name, x.Value, x.Synonyms))
                .ToList(),
            
            message.FileNotes
                .Select(x =>
                    new ActualizeNote(x.SourceId, x.Label, x.PositionFromLeft, x.PositionFromTop, x.Width, x.Height))
                .ToList());

        await _mediator.Send(command);
    }

    private static ImoutoRebirth.Lilin.Core.FileInfoAggregate.MetadataSource Convert(MetadataSource metadata)
        => (ImoutoRebirth.Lilin.Core.FileInfoAggregate.MetadataSource) (int) metadata;
}
