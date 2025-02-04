using ImoutoRebirth.Meido.Application.ParsingStatusSlice.Commands;
using ImoutoRebirth.Meido.Domain;
using ImoutoRebirth.Meido.MessageContracts;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Meido.UI.Consumers;

public class SearchCompleteCommandConsumer : IConsumer<SearchCompleteCommand>
{
    private readonly IMediator _mediator;

    public SearchCompleteCommandConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<SearchCompleteCommand> context)
    {
        var command = new SaveCompletedSearchCommand(
            (MetadataSource)context.Message.SourceId,
            context.Message.FileId,
            (Domain.ParsingStatusAggregate.SearchStatus)context.Message.ResultStatus,
            context.Message.ErrorText,
            context.Message.FileIdFromSource);

        await _mediator.Send(command);
    }
}
