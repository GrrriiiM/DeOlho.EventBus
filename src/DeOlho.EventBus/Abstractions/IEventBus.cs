using System;
using System.Threading.Tasks;
using DeOlho.EventBus.Manager;

namespace DeOlho.EventBus.Abstractions
{
    public interface IEventBus
    {
        void Publish<TMessage>(TMessage message) where TMessage : EventBusMessage;
        void Publish(Type messageType, EventBusMessage message);
        void Subscribe<TMessage>(Func<EventBusSubscriptionContext, TMessage, Task> onMessage) where TMessage : EventBusMessage;
        void Subscribe(Type messageType, Func<EventBusSubscriptionContext, EventBusMessage, Task> onMessage);
        void SubscribeFail<TMessage>(Func<EventBusSubscriptionContext, TMessage, Task> onMessage) where TMessage : EventBusMessage;
        void SubscribeFail(Type messageType, Func<EventBusSubscriptionContext, EventBusMessage, Task> onMessage);
        void Unsubscribe<TMessage>() where TMessage : EventBusMessage;
        void Unsubscribe(Type message);
    }
}