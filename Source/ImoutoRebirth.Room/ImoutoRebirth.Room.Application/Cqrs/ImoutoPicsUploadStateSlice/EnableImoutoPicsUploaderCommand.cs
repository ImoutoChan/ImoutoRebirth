using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Room.Application.Services;

namespace ImoutoRebirth.Room.Application.Cqrs.ImoutoPicsUploadStateSlice;

public record EnableImoutoPicsUploaderCommand : ICommand;

public record DisableImoutoPicsUploaderCommand : ICommand;

internal class EnableImoutoPicsUploaderCommandHandler 
    : ICommandHandler<EnableImoutoPicsUploaderCommand>
    , ICommandHandler<DisableImoutoPicsUploaderCommand>
{
    private readonly IImoutoPicsUploaderRepository _imoutoPicsUploaderRepository;

    public EnableImoutoPicsUploaderCommandHandler(IImoutoPicsUploaderRepository imoutoPicsUploaderRepository) 
        => _imoutoPicsUploaderRepository = imoutoPicsUploaderRepository;

    public Task Handle(EnableImoutoPicsUploaderCommand _, CancellationToken ct)
    {
        var state = _imoutoPicsUploaderRepository.Get();
        state.Enable();
        
        return Task.CompletedTask;
    }
    
    public Task Handle(DisableImoutoPicsUploaderCommand _, CancellationToken ct)
    {
        var state = _imoutoPicsUploaderRepository.Get();
        state.Disable();
        
        return Task.CompletedTask;
    }
}
