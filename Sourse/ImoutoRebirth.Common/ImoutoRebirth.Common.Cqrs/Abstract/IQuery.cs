using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Abstract;

public interface IQuery<out TResult> : IRequest<TResult>
{
}

public interface IStreamQuery<out TResult> : IStreamRequest<TResult>
{
}
