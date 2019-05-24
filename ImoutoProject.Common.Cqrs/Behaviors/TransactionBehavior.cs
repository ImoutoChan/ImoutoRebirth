using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Domain;
using MediatR;

namespace ImoutoProject.Common.Cqrs.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventStorage _eventStorage;
        private readonly IMediator _mediator;

        public TransactionBehavior(
            IUnitOfWork unitOfWork, 
            IEventStorage eventStorage, 
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _eventStorage = eventStorage;
            _mediator = mediator;
        }

        public async Task<TResponse> Handle(
            TRequest request, 
            CancellationToken cancellationToken, 
            RequestHandlerDelegate<TResponse> next)
        {
            TResponse response;
            using (await _unitOfWork.CreateTransaction(IsolationLevel.ReadCommitted))
            {
                response = await next();

                await _unitOfWork.SaveEntitiesAsync(cancellationToken);
                _unitOfWork.CommitTransaction();
            }

            foreach (var domainEvent in _eventStorage.GetAll())
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }

            return response;
        }
    }
}