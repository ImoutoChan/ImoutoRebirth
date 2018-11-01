using MediatR;

namespace ImoutoRebirth.Lilin.Services.CQRS.Abstract
{
    public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
    }
}