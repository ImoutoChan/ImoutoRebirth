using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Lilin.Core.Infrastructure;
using MediatR;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands
{
    public class UpdateTagsCountersCommandHandler : ICommandHandler<UpdateTagsCountersCommand>
    {
        private readonly ITagRepository _tagRepository;

        public UpdateTagsCountersCommandHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<Unit> Handle(UpdateTagsCountersCommand request, CancellationToken cancellationToken)
        {
            await _tagRepository.UpdateTagsCounters();

            return Unit.Value;
        }
    }
}