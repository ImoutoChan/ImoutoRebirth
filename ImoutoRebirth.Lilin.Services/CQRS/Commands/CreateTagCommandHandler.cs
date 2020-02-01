using System;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class CreateTagCommandHandler : ICommandHandler<CreateTagCommand, Tag>
    {
        private readonly ITagRepository _tagRepository;
        private readonly ITagTypeRepository _tagTypeRepository;

        public CreateTagCommandHandler(ITagRepository tagRepository, ITagTypeRepository tagTypeRepository)
        {
            _tagRepository = tagRepository;
            _tagTypeRepository = tagTypeRepository;
        }

        public async Task<Tag> Handle(CreateTagCommand request, CancellationToken cancellationToken)
        {
            var tag = await _tagRepository.Get(request.Name, request.TypeId);

            if (tag != null)
            {
                tag.UpdateHasValue(request.HasValue);

                if (request.Synonyms != null)
                    tag.UpdateSynonyms(request.Synonyms);

                await _tagRepository.Update(tag);
            }
            else
            {
                var tagType = await _tagTypeRepository.Get(request.TypeId);

                if (tagType == null)
                    throw new ApplicationException($"TagType not found id: {request.TypeId}.");

                tag = Tag.CreateNew(tagType, request.Name, request.HasValue, request.Synonyms);

                await _tagRepository.Create(tag);
            }

            var resultTag = await _tagRepository.Get(request.Name, request.TypeId);

            if (resultTag == null)
                throw new ApplicationException($"Tag was not created.");

            return resultTag;
        }
    }
}