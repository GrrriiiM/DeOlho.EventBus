using System;
using System.Threading.Tasks;
using DeOlho.EventBus.Message;

namespace DeOlho.EventBus.Manager
{
    public class EventBusSubscription
    {
        protected readonly Func<EventBusSubscriptionContext, EventBusMessage, Task> _onMessage;

        public EventBusSubscription(Type messageType, EventBusManager manager, Func<EventBusSubscriptionContext, EventBusMessage, Task> onMessage, bool isSubscriptionForFail = false)
        {
            MessageType = messageType;
            _onMessage = onMessage;
            IsSubscriptionForFail = isSubscriptionForFail;
            SubscriptionEventName = manager.GetEventName(messageType, IsSubscriptionForFail);
        }

        public string SubscriptionEventName { get; private set; }
        public bool IsSubscriptionForFail { get; private set; }
        public Type MessageType { get; protected set; }
        
        public virtual Task OnMessage(EventBusMessage message, string[] exceptionMessageStackForFailSubscription = null)
        {
            return _onMessage(new EventBusSubscriptionContext(this, exceptionMessageStackForFailSubscription), message);
        }
    }

    public class EventBusSubscription<TMessage> : EventBusSubscription where TMessage : EventBusMessage
    {
        public EventBusSubscription(EventBusManager manager, Func<EventBusSubscriptionContext, TMessage, Task> onMessage, bool isSubscriptionForFail = false)
            : base(typeof(TMessage), manager, (c, m) => onMessage(c, (TMessage)m), isSubscriptionForFail)
        {
        }
    }

    public class EventBusSubscriptionContext
    {
        internal EventBusSubscriptionContext(
            EventBusSubscription subscription, 
            string[] exceptionMessageStack)
        {
            MessageType = subscription.MessageType;
            MessageName = subscription.SubscriptionEventName;
            IsSubscriptionForFail = subscription.IsSubscriptionForFail;
            ExceptionMessageStack = exceptionMessageStack;
        }

        public string MessageName { get; private set; }
        public bool IsSubscriptionForFail { get; private set; }
        public Type MessageType { get; private set; }
        public string[] ExceptionMessageStack { get; private set; }
    }
}