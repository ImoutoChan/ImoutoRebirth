using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.MessageContracts;
using ImoutoRebirth.Lilin.Services.CQRS.Commands;
using MassTransit;
using MediatR;
using MetadataSource = ImoutoRebirth.Lilin.Core.Models.MetadataSource;

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
            var metadataUpdate = ToMetadataUpdate(context.Message);

            var command = new SaveMetadataCommand(metadataUpdate);
            await _mediator.Send(command);
        }

        private MetadataUpdate ToMetadataUpdate(IUpdateMetadataCommand message)
        {
            var tags = message.FileTags.Select(
                                   x => new FileTagBind(
                                       message.FileId,
                                       x.TagId,
                                       x.Value,
                                       (MetadataSource) (int) message.MetadataSource))
                              .ToArray();

            var notes = message.FileNotes.Select(
                                    x => new FileNote(
                                        message.FileId,
                                        new Note(
                                            Guid.NewGuid(),
                                            x.Label,
                                            x.PositionFromLeft,
                                            x.PositionFromTop,
                                            x.Width,
                                            x.Height),
                                        (MetadataSource) (int) message.MetadataSource,
                                        x.SourceId))
                               .ToArray();

            return new MetadataUpdate(message.FileId, tags, notes);
        }
    }
}
