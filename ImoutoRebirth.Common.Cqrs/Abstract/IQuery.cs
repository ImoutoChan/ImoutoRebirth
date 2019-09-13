using MediatR;

namespace ImoutoProject.Common.Cqrs.Abstract
{
    public interface IQuery<out TResult> : IRequest<TResult>
    {
    }
}