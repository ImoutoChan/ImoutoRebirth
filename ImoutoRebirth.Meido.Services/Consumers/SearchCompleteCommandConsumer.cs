using System.Threading.Tasks;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.Services.Cqrs.Commands;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Meido.Services.Consumers
{
    public class SearchCompleteCommandConsumer : IConsumer<ISearchCompleteCommand>
    {
        private readonly IMediator _mediator;

        public SearchCompleteCommandConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<ISearchCompleteCommand> context)
        {
            var command = new SearchCompleteCommand(
                context.Message.SourceId,
                context.Message.FileId,
                context.Message.ResultStatus,
                context.Message.ErrorText,
                context.Message.FileIdFromSource);

            await _mediator.Send(command);
        }
    }
}