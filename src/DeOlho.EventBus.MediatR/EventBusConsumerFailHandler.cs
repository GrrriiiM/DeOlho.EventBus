using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Message;
using MediatR;

namespace DeOlho.EventBus.MediatR
{
    public abstract class EventBusConsumerFailHandler<TMessage> : IRequestHandler<EventBusConsumerFail<TMessage>> 
        where TMessage : EventBusMessage
    {
        public Task<Unit> Handle(EventBusConsumerFail<TMessage> request, CancellationToken cancellationToken)
        {
            return Handle(request.Message, request.ExceptionStack, cancellationToken);
        }

        public abstract Task<Unit> Handle(TMessage message, string[] exceptionStack, CancellationToken cancellationToken);
    }
}