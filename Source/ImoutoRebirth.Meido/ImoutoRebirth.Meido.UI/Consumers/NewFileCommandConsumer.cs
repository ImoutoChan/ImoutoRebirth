using ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Meido.UI.Consumers;

internal class NewFileCommandConsumer : IConsumer<INewFileCommand>
{
    private readonly IMediator _mediator;

    public NewFileCommandConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<INewFileCommand> context) 
        => await _mediator.Send(
            new CreateParsingsForNewFileCommand(context.Message.FileId, context.Message.Md5));
}
