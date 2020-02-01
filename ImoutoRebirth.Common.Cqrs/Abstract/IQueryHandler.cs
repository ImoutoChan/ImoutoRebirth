using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Abstract
{
    public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
    }
}