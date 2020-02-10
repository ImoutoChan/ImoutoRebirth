using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;
using ImoutoRebirth.Lilin.Core.Models.FileInfoAggregate;
using ImoutoRebirth.Lilin.Services.ApplicationServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class BindTagsToFilesCommandHandler : ICommandHandler<BindTagsToFilesCommand>
    {
        private readonly IFileTagRepository _fileTagRepository;
        private readonly ILogger<BindTagsToFilesCommandHandler> _logger;
        private readonly IFileInfoService _fileInfoService;
        private readonly ITagRepository _tagRepository;

        public BindTagsToFilesCommandHandler(
            IFileTagRepository fileTagRepository, 
            ILogger<BindTagsToFilesCommandHandler> logger,
            IFileInfoService fileInfoService,
            ITagRepository tagRepository)
        {
            _fileTagRepository = fileTagRepository;
            _logger = logger;
            _fileInfoService = fileInfoService;
            _tagRepository = tagRepository;
        }

        public async Task<Unit> Handle(BindTagsToFilesCommand request, CancellationToken cancellationToken)
        {
            if (!request.FileTags.Any())
            {
                _logger.LogWarning("Trying to batch add {FileTagCount} new FileTags", request.FileTags.Count);
                return Unit.Value;
            }

            var tags = await LoadTags(request);
            var fileInfos = await LoadFileInfos(request);

            var newFileTags = request
                .FileTags
                .Select(
                    x => new FileTag(
                        x.FileId,
                        tags[x.TagId],
                        x.Value,
                        x.Source))
                .ToArray();
            
            var fileInfoPack = new FileInfoPack(fileInfos);
            fileInfoPack.UpdateTags(newFileTags);

            foreach (var file in fileInfoPack.Files)
            {
                await _fileInfoService.PersistFileAggregate(file);
            }

            _logger.LogInformation("Batch added {FileTagCount} new FileTags", request.FileTags.Count);
            return Unit.Value;
        }

        private async Task<IReadOnlyCollection<FileInfo>> LoadFileInfos(BindTagsToFilesCommand request)
        {
            var fileIds = request
                .FileTags
                .Select(x => x.FileId)
                .Distinct()
                .ToArray();

            var fileInfos = new List<FileInfo>(fileIds.Length);
            foreach (var fileId in fileIds)
            {
                var fileInfo = await _fileInfoService.LoadFileAggregate(fileId);
                fileInfos.Add(fileInfo);
            }
            
            return fileInfos;
        }

        private async Task<Dictionary<Guid, Tag>> LoadTags(BindTagsToFilesCommand request)
        {
            var tagIds = request
                .FileTags
                .Select(x => x.TagId)
                .Distinct()
                .ToArray();

            var tags = new Dictionary<Guid, Tag>(tagIds.Length);
            foreach (var tagId in tagIds)
            {
                var tag = await _tagRepository.Get(tagId);
                if (tag == null)
                    throw new ApplicationException($"Tag with id {tagId} is not found");

                tags.Add(tagId, tag);
            }

            return tags;
        }
    }
}