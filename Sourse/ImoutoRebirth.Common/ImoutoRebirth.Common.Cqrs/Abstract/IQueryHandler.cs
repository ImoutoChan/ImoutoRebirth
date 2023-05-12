using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Abstract;

public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
}

public interface IStreamQueryHandler<in TQuery, out TResult> : IStreamRequestHandler<TQuery, TResult>
    where TQuery : IStreamQuery<TResult>
{
}
