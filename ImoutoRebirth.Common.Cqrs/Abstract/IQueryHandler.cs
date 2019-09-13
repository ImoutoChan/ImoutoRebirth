using MediatR;

namespace ImoutoProject.Common.Cqrs.Abstract
{
    public interface IQueryHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
    }
}