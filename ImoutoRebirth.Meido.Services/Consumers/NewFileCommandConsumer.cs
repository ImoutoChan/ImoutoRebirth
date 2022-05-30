using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.Services.Cqrs.Commands;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Meido.Services.Consumers;

public class NewFileCommandConsumer : IConsumer<INewFileCommand>
{
    private readonly IMediator _mediator;

    public NewFileCommandConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<INewFileCommand> context)
    {
        var command = new AddNewFileCommand(context.Message.FileId, context.Message.Md5);
        await _mediator.Send(command);
    }
}