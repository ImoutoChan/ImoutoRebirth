using ImoutoRebirth.Common.Cqrs.Abstract;
using ImoutoRebirth.Meido.Core.FaultTolerance;
using MediatR;

namespace ImoutoRebirth.Meido.Services.FaultTolerance.CqrsCommands;

internal class RequeueFaultsCommandHandler : ICommandHandler<RequeueFaultsCommand>
{
    private readonly IFaultToleranceService _faultToleranceService;

    public RequeueFaultsCommandHandler(IFaultToleranceService faultToleranceService)
    {
        _faultToleranceService = faultToleranceService;
    }
        
    public async Task Handle(RequeueFaultsCommand request, CancellationToken cancellationToken)
    {
        await _faultToleranceService.RequeueFaults();
    }
}
