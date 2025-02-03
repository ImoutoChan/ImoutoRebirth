using ImoutoRebirth.Common.Application;
using MassTransit;

namespace ImoutoRebirth.Common.Infrastructure;

internal class DistributedCommandBus : IDistributedCommandBus
{
    private readonly IBus _bus;

    public DistributedCommandBus(IBus bus) => _bus = bus;

    public async Task SendAsync<TCommand>(TCommand command, CancellationToken token = default) where TCommand : class
        => await _bus.Send(command, cancellationToken: token);
}
