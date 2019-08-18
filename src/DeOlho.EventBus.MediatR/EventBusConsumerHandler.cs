using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Message;
using MediatR;

namespace DeOlho.EventBus.MediatR
{
    public abstract class EventBusConsumerHandler<TMessage> : IRequestHandler<EventBusConsumer<TMessage>>, IEventBusConsumerHandler
        where TMessage : EventBusMessage
    {
        public Task<Unit> Handle(EventBusConsumer<TMessage> request, CancellationToken cancellationToken)
        {
            return Handle(request.Message, cancellationToken);
        }

        public abstract Task<Unit> Handle(TMessage message, CancellationToken cancellationToken);
    }

    public interface IEventBusConsumerHandler
    {}
}