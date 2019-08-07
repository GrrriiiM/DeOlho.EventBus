using System;

namespace DeOlho.EventBus
{
    public class EventBusMessageFail<TEventBusMessage> : EventBusMessage where TEventBusMessage : EventBusMessage
    {
        public EventBusMessageFail(TEventBusMessage message)
            : base(message.MessageId)
        {   
            Message = message;
        }

        public TEventBusMessage Message { get; private set; }
    }
}