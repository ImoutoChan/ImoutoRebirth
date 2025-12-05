using System.Data;
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
        CancellationToken ct)
    {
        var isolationLevel = typeof(TRequest).GetIsolationLevel();

        if (isolationLevel == IsolationLevel.Unspecified)
            return await next(ct);

        using var transaction = await _unitOfWork.CreateTransactionAsync(isolationLevel);

        var response = await next(ct);

        await _unitOfWork.SaveEntitiesAsync(ct);
        await transaction.CommitAsync();

        return response;
    }
}
