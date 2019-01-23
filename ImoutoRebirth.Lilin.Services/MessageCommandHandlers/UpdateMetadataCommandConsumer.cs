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
            var metadataUpdate = ToMetadataUpdate(context.Message);

            var command = new SaveMetadataCommand(metadataUpdate);
            await _mediator.Send(command);
        }

        private static MetadataUpdate ToMetadataUpdate(IUpdateMetadataCommand command)
        {
            var tags = command.FileTags.Select(
                                   x => new FileTagBind(
                                       command.FileId,
                                       command.MetadataSource.Convert(),
                                       x.Type,
                                       x.Name,
                                       x.Value,
                                       x.Synonyms))
                              .ToArray();

            var notes = command.FileNotes.Select(
                                    x => new FileNote(
                                        command.FileId,
                                        new Note(
                                            Guid.NewGuid(),
                                            x.Label,
                                            x.PositionFromLeft,
                                            x.PositionFromTop,
                                            x.Width,
                                            x.Height),
                                        command.MetadataSource.Convert(),
                                        x.SourceId))
                               .ToArray();

            return new MetadataUpdate(command.FileId, tags, notes, command.MetadataSource.Convert());
        }
    }
}
