using MediatR;

namespace ImoutoRebirth.Lilin.Services.CQRS.Abstract
{
    public interface IQuery<out TResult> : IRequest<TResult>
    {
    }
}