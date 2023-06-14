namespace ImoutoRebirth.Common.Application;

public interface IDistributedCommandBus
{
    Task SendAsync<TCommand>(object command, CancellationToken token = default)
        where TCommand : class;
}