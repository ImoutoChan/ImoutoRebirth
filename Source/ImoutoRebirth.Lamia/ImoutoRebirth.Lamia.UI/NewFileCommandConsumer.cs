using ImoutoRebirth.Lamia.Application.Cqrs;
using ImoutoRebirth.Lamia.MessageContracts;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Lamia.UI;

public class ExtractFileMetadataCommandConsumer : IConsumer<ExtractFileMetadataCommand>
{
    private readonly IMediator _mediator;

    public ExtractFileMetadataCommandConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<ExtractFileMetadataCommand> context)
        => await _mediator.Send(new ExtractMetadataCommand(context.Message.FileId, context.Message.FileFullName));
}
