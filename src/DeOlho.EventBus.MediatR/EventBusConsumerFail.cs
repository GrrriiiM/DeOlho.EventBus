using DeOlho.EventBus.Message;
using MediatR;

namespace DeOlho.EventBus.MediatR
{
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