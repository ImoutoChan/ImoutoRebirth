using ImoutoRebirth.Common.Domain;
using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var isolationLevel = typeof(TRequest).GetIsolationLevel();

        using var transaction = await _unitOfWork.CreateTransactionAsync(isolationLevel);

        var response = await next();

        await _unitOfWork.SaveEntitiesAsync(cancellationToken);
        await transaction.CommitAsync();

        return response;
    }
}
