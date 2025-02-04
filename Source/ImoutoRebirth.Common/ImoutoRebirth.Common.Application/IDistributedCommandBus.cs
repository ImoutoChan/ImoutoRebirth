namespace ImoutoRebirth.Common.Application;

public interface IDistributedCommandBus
{
    Task SendAsync<TCommand>(TCommand command, CancellationToken token = default)
        where TCommand : class;
}
