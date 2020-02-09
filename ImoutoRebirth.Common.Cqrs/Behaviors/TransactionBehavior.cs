using System.Threading;
using System.Threading.Tasks;
using ImoutoRebirth.Common.Cqrs.Events;
using ImoutoRebirth.Common.Domain;
using MediatR;

namespace ImoutoRebirth.Common.Cqrs.Behaviors
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
            var isolationLevel = typeof(TRequest).GetIsolationLevel();

            TResponse response;
            using (var transaction = await _unitOfWork.CreateTransactionAsync(isolationLevel))
            {
                response = await next();

                await _unitOfWork.SaveEntitiesAsync(cancellationToken);
                await transaction.CommitAsync();
            }

            foreach (var domainEvent in _eventStorage.GetAll())
            {
                await _eventPublisher.Publish(domainEvent, cancellationToken);
            }

            return response;
        }
    }
}