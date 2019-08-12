using System;

namespace DeOlho.EventBus.Message
{
    public class EventBusMessageFail<TEventBusMessage> : EventBusMessage where TEventBusMessage : EventBusMessage
    {
        public EventBusMessageFail(TEventBusMessage message, string[] exceptionMessageStack)
            : base(message.MessageId)
        {   
            Message = message;
            ExceptionMessageStack = exceptionMessageStack;
        }

        public TEventBusMessage Message { get; private set; }
        public string[] ExceptionMessageStack { get; private set; }
    }
}