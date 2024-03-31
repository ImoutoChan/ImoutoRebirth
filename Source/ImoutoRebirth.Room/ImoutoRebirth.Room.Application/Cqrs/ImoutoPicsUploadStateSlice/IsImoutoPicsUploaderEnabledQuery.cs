using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.ImoutoPicsUploadStateSlice;

public record IsImoutoPicsUploaderEnabledQuery : IQuery<bool>;

internal class IsImoutoPicsUploaderEnabledQueryHandler 
    : IQueryHandler<IsImoutoPicsUploaderEnabledQuery, bool>
{
    private readonly IImoutoPicsUploaderRepository _imoutoPicsUploaderRepository;

    public IsImoutoPicsUploaderEnabledQueryHandler(IImoutoPicsUploaderRepository imoutoPicsUploaderRepository) 
        => _imoutoPicsUploaderRepository = imoutoPicsUploaderRepository;

    public Task<bool> Handle(IsImoutoPicsUploaderEnabledQuery _, CancellationToken ct)
    {
        var state = _imoutoPicsUploaderRepository.Get();
        return Task.FromResult(state.IsEnabled);
    }
}
