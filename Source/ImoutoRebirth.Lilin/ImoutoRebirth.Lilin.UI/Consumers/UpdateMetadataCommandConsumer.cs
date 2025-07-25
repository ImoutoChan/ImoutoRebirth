﻿using ImoutoRebirth.Lilin.Application.FileInfoSlice.Commands;
using ImoutoRebirth.Lilin.Domain.TagAggregate;
using ImoutoRebirth.Lilin.MessageContracts;
using MassTransit;
using MediatR;

namespace ImoutoRebirth.Lilin.UI.Consumers;

public class UpdateMetadataCommandConsumer : IConsumer<UpdateMetadataCommand>
{
    private readonly IMediator _mediator;

    public UpdateMetadataCommandConsumer(IMediator mediator) => _mediator = mediator;

    public async Task Consume(ConsumeContext<UpdateMetadataCommand> context)
    {
        var message = context.Message;

        var command = new ActualizeFileInfoForSourceCommand(
            message.FileId,
            Convert(message.MetadataSource),

            message.FileTags
                ?.Select(x => new ActualizeTag(x.Type, x.Name, x.Value, x.Synonyms, TagOptions.None))
                .ToArray() ?? [],

            message.FileNotes
                ?.Select(x =>
                    new ActualizeNote(x.SourceId, x.Label, x.PositionFromLeft, x.PositionFromTop, x.Width, x.Height))
                .ToArray() ?? []);

        await _mediator.Send(command);
    }

    private static ImoutoRebirth.Lilin.Domain.FileInfoAggregate.MetadataSource Convert(MetadataSource metadata)
    {
        return metadata switch
        {
            MetadataSource.Yandere => Domain.FileInfoAggregate.MetadataSource.Yandere,
            MetadataSource.Danbooru => Domain.FileInfoAggregate.MetadataSource.Danbooru,
            MetadataSource.Sankaku => Domain.FileInfoAggregate.MetadataSource.Sankaku,
            MetadataSource.Manual => Domain.FileInfoAggregate.MetadataSource.Manual,
            MetadataSource.Gelbooru => Domain.FileInfoAggregate.MetadataSource.Gelbooru,
            MetadataSource.Rule34 => Domain.FileInfoAggregate.MetadataSource.Rule34,
            MetadataSource.ExHentai => Domain.FileInfoAggregate.MetadataSource.ExHentai,
            MetadataSource.Schale => Domain.FileInfoAggregate.MetadataSource.Schale,
            MetadataSource.Lamia => Domain.FileInfoAggregate.MetadataSource.Lamia,
            _ => throw new ArgumentOutOfRangeException(nameof(metadata), metadata, null)
        };
    }
}
