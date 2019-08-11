using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace DeOlho.EventBus.RabbitMQ.AspNetCore
{
    public abstract class EventBusConsumerHandler<TMessage> : IRequestHandler<EventBusConsumer<TMessage>> where TMessage : EventBusMessage
    {
        public Task<Unit> Handle(EventBusConsumer<TMessage> request, CancellationToken cancellationToken)
        {
            return Handle(request.Message, cancellationToken);
        }

        public abstract Task<Unit> Handle(TMessage message, CancellationToken cancellationToken);
    }

    public abstract class EventBusConsumerFailHandler<TMessage> : IRequestHandler<EventBusConsumerFail<TMessage>> where TMessage : EventBusMessage
    {
        public Task<Unit> Handle(EventBusConsumerFail<TMessage> request, CancellationToken cancellationToken)
        {
            return Handle(request.Message, request.ExceptionStack, cancellationToken);
        }

        public abstract Task<Unit> Handle(TMessage message, string[] exceptionStack, CancellationToken cancellationToken);
    }

    public class EventBusConsumer<TMessage> : IRequest where TMessage : EventBusMessage 
    {
        public EventBusConsumer(TMessage message)
        {
            Message = message;
        }
        public TMessage Message { get; private set; }
    }

    public class EventBusConsumerFail<TMessage> : IRequest where TMessage : EventBusMessage 
    {
        public EventBusConsumerFail(TMessage message, string[] exceptionStack)
        {
            Message = message;
            ExceptionStack = exceptionStack;
        }
        public TMessage Message { get; private set; }
        public string[] ExceptionStack { get; private set; }
    }
}