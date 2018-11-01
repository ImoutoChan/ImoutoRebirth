using MediatR;

namespace ImoutoRebirth.Lilin.Services.CQRS.Abstract
{
    public interface ICommand<out TResult> : IRequest<TResult>
    {
    }

    public interface ICommand : IRequest
    {
    }
}