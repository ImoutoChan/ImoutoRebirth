using System;
using System.Linq;
using System.Threading.Tasks;
using ImoutoRebirth.Arachne.Core;
using ImoutoRebirth.Arachne.Core.Models;
using ImoutoRebirth.Arachne.MessageContracts;
using ImoutoRebirth.Arachne.Service.Commands;
using ImoutoRebirth.Arachne.Service.Extensions;
using ImoutoRebirth.Lilin.MessageContracts;
using Mackiovello.Maybe;
using MassTransit;

namespace ImoutoRebirth.Arachne.Service
{
    public class SearchMetadataCommandHandler : ISearchMetadataCommandHandler
    {
        private readonly IArachneSearchService _arachneSearchService;
        private readonly IRemoteCommandService _remoteCommandService;

        public SearchMetadataCommandHandler(IArachneSearchService arachneSearchService, IRemoteCommandService remoteCommandService)
        {
            _arachneSearchService = arachneSearchService;
            _remoteCommandService = remoteCommandService;
        }

        public async Task Search(ConsumeContext<ISearchMetadataCommand> context, SearchEngineType where)
        {
            var md5 = context.Message.Md5;

            var searchResults = await _arachneSearchService.Search(new Image(md5), where);

            var sendCommand = ConvertToCommand(searchResults, context.Message.FileId)
                .Select(command => _remoteCommandService.SendCommand<IUpdateMetadataCommand>(command));

            if (sendCommand.HasValue)
                await sendCommand.Value;

            // todo debug only
#if DEBUG
            if (searchResults is Metadata searchResult)
                Console.Out.WriteLine(searchResult.Source + " | " + searchResult.IsFound);
#endif
        }

        private Maybe<UpdateMetadataCommand> ConvertToCommand(SearchResult searchResults, Guid fileId)
        {
            if (!(searchResults is Metadata searchResult) || !searchResult.IsFound)
                return Maybe<UpdateMetadataCommand>.Nothing;


            var tags = searchResult.Tags.Select(CreateFileTag).ToArray();
            var notes = searchResult.Notes.Select(CreateFileNote).ToArray();

            return new UpdateMetadataCommand
                   {
                       FileId = fileId,
                       FileTags = tags,
                       FileNotes = notes,
                       MetadataSource = searchResult.Source.GetMetadataSource()
                   }.ToMaybe();

        }

        private IFileTag CreateFileTag(Tag tag) 
            => new FileTag
            {
                Name = tag.Name,
                Type = tag.Type,
                Synonyms = tag.Synonyms.ToArray(),
                Value = tag.Value
            };

        private IFileNote CreateFileNote(Note note)
            => new FileNote
               {
                   SourceId = note.SourceId,
                   Label = note.Label,
                   PositionFromLeft = note.Position.PointLeft,
                   PositionFromTop = note.Position.PointTop,
                   Width = note.Position.SizeWidth,
                   Height = note.Position.SizeHeight
               };
    }
}