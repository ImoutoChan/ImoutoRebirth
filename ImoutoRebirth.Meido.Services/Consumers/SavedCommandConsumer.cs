using System.Threading.Tasks;
using ImoutoRebirth.Meido.MessageContracts;
using ImoutoRebirth.Meido.Services.Cqrs.Commands;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Meido.Services.Consumers
{
    public class SavedCommandConsumer : IConsumer<ISavedCommand>
    {
        private readonly IMediator _mediator;

        public SavedCommandConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<ISavedCommand> context)
        {
            var command = new MarkMetadataSavedCommand(context.Message.FileId, context.Message.SourceId);

            await _mediator.Send(command);
        }
    }
}