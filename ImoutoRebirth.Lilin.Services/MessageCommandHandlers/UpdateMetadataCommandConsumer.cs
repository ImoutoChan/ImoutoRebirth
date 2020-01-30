using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services.CQRS.Commands;
using ImoutoRebirth.Lilin.Services.Extensions;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Lilin.Services.MessageCommandHandlers
{
    public class UpdateMetadataCommandConsumer : IConsumer<IUpdateMetadataCommand>
    {
        private readonly IMediator _mediator;

        public UpdateMetadataCommandConsumer(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Consume(ConsumeContext<IUpdateMetadataCommand> context)
        {
            var command = new SaveMetadataCommand(context.Message);
            await _mediator.Send(command);
        }
    }
}
