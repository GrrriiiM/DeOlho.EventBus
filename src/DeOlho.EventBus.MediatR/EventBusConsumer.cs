using DeOlho.EventBus.Message;
using MediatR;

namespace DeOlho.EventBus.MediatR
{
    public class EventBusConsumer<TMessage> : IRequest where TMessage : EventBusMessage 
    {
        public EventBusConsumer(TMessage message)
        {
            Message = message;
        }
        public TMessage Message { get; private set; }
    }
}