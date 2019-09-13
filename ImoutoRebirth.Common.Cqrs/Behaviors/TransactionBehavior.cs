using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ImoutoProject.Common.Cqrs.Events;
using ImoutoRebirth.Common.Domain;
using MediatR;

namespace ImoutoProject.Common.Cqrs.Behaviors
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventStorage _eventStorage;
        private readonly IEventPublisher _eventPublisher;

        public TransactionBehavior(
            IUnitOfWork unitOfWork, 
            IEventStorage eventStorage,
            IEventPublisher eventPublisher)
        {
            _unitOfWork = unitOfWork;
            _eventStorage = eventStorage;
            _eventPublisher = eventPublisher;
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
                await _eventPublisher.Publish(domainEvent, cancellationToken);
            }

            return response;
        }
    }
}