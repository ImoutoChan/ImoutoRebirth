using System.Data;
using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Common.Cqrs.Behaviors;
using ImoutoRebirth.Lilin.Core.Infrastructure;

namespace ImoutoRebirth.Lilin.Services.CQRS.Commands;

[CommandQuery(IsolationLevel.ReadCommitted)]
public record UpdateTagsCountersCommand : ICommand;

internal class UpdateTagsCountersCommandHandler : ICommandHandler<UpdateTagsCountersCommand>
{
    private readonly ITagRepository _tagRepository;

    public UpdateTagsCountersCommandHandler(ITagRepository tagRepository) => _tagRepository = tagRepository;

    public Task Handle(UpdateTagsCountersCommand request, CancellationToken cancellationToken) 
        => _tagRepository.UpdateTagsCounters();
}
