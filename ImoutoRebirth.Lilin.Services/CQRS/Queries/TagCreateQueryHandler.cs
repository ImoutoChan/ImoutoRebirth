using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using ImoutoRebirth.Lilin.Core.Models;

namespace ImoutoRebirth.Lilin.Services.CQRS.Queries
{
    public class TagCreateQueryHandler : IQueryHandler<TagCreateQuery, Tag>
    {
        private readonly ITagRepository _tagRepository;

        public TagCreateQueryHandler(ITagRepository tagRepository) => _tagRepository = tagRepository;

        public async Task<Tag> Handle(TagCreateQuery request, CancellationToken cancellationToken) 
            => await _tagRepository.GetOrCreate(request.Name, request.TypeId, request.HasValue, request.Synonyms);
    }
}