using ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;
using ImoutoRebirth.Lilin.MessageContracts;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Lilin.UI.Consumers;

public class UpdateLocationTagsCommandConsumer : IConsumer<UpdateLocationTagsCommand>
{
    private readonly IMediator _mediator;

    public UpdateLocationTagsCommandConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<UpdateLocationTagsCommand> context)
    {
        var message = context.Message;

        var command = new ActualizeLocationTagsCommand(message.FileId, message.LocationTags);
        await _mediator.Send(command);
    }
}
