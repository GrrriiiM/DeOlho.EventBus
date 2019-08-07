using System;
using System.Threading.Tasks;

namespace DeOlho.EventBus.Abstractions
{
    public interface IEventBus
    {
        void Publish<TMessage>(TMessage message) where TMessage : EventBusMessage;
        void Publish(Type messageType, EventBusMessage message);
        void Subscribe<TMessage>(Func<TMessage, Task> onMessage) where TMessage : EventBusMessage;
        void Subscribe(Type messageType, Func<EventBusMessage, Task> onMessage);
        void Unsubscribe<TMessage>() where TMessage : EventBusMessage;
        void Unsubscribe(Type message);
    }
}