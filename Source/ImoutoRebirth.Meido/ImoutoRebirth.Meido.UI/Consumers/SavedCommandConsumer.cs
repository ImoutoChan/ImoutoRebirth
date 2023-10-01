using ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Meido.UI.Consumers;

internal class SavedCommandConsumer : IConsumer<ISavedCommand>
{
    private readonly IMediator _mediator;

    public SavedCommandConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<ISavedCommand> context)
    {
        var command = new MarkMetadataSavedCommand(context.Message.FileId, (MetadataSource)context.Message.SourceId);
        await _mediator.Send(command);
    }
}
