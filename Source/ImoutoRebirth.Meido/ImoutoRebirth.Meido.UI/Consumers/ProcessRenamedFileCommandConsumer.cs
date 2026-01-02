using ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Meido.UI.Consumers;

public class ProcessRenamedFileCommandConsumer : IConsumer<ProcessRenamedFileCommand>
{
    private readonly IMediator _mediator;

    public ProcessRenamedFileCommandConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<ProcessRenamedFileCommand> context)
        => await _mediator.Send(
            new RecreateParsingsForFileWithNewFileNameCommand(
                context.Message.FileId,
                context.Message.Md5,
                context.Message.NewFileName));
}
