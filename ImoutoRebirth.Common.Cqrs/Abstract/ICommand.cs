using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Abstract
{
    public interface ICommand<out TResult> : IRequest<TResult>
    {
    }

    public interface ICommand : IRequest
    {
    }
}