using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DeOlho.EventBus.Manager
{
    public class EventBusManager
    {
        

        readonly List<EventBusSubscription> _subscriptions;
        readonly ILogger<EventBusManager> _logger;

        public EventBusManager(
            ILogger<EventBusManager> logger)
        {
            _logger = logger;
            _subscriptions = new List<EventBusSubscription>();
        }

        public IReadOnlyList<EventBusSubscription> Subscriptions => _subscriptions.AsReadOnly();

        public bool HasSubscription(string messageTypeName)
        {
            return _subscriptions.Any(_ => _.MessageTypeName == messageTypeName);
        }

        public bool HasSubscription(Type messageType)
        {
            return HasSubscription(GetMessageExchangeName(messageType));
        }

        public bool HasSubscription<TMessage>()
        {
            return HasSubscription(typeof(TMessage));
        }

        public EventBusSubscription GetSubscription(string messageTypeName)
        {
            return _subscriptions.SingleOrDefault(_ => _.MessageTypeName == messageTypeName);
        }

        public void AddSubscription<TMessage>(Func<TMessage, Task> onMessage) where TMessage : EventBusMessage
        {
            if (!HasSubscription<TMessage>())
            {
                _subscriptions.Add(new EventBusSubscription<TMessage>(onMessage));
            }
            else
            {
                _logger.LogWarning($"Já existe uma assinatura para {typeof(TMessage).Name}");
            }
        }

        public void AddSubscription(Type messageType, Func<EventBusMessage, Task> onMessage)
        {
            if (!HasSubscription(messageType))
            {
                _subscriptions.Add(new EventBusSubscription(messageType, onMessage));
            }
            else
            {
                _logger.LogWarning($"Já existe uma assinatura para {messageType.Name}");
            }
        }

        public void Clear()
        {
            _subscriptions.Clear();
        }


        public static string GetMessageExchangeName<TMessage>() where TMessage : EventBusMessage
        {
            return GetMessageExchangeName(typeof(TMessage));
        }

        public static string GetMessageExchangeName(Type messageType)
        {
            if (!typeof(EventBusMessage).IsAssignableFrom(messageType))
            {
                throw new ArgumentException(nameof(messageType));
            }
            else if (messageType.IsGenericType && typeof(EventBusMessageFail<>).IsAssignableFrom(messageType.GetGenericTypeDefinition()))
            {
                return $"{messageType.GetGenericArguments()[0].FullName}_fail";
            }
            else
            {
                return messageType.FullName;
            }
        }
    }

    public class EventBusSubscription
    {
        protected readonly Func<EventBusMessage, Task> _onMessage;

        public EventBusSubscription(Type messageType, Func<EventBusMessage, Task> onMessage)
        {
            MessageType = messageType;
            _onMessage = onMessage;
        }

        public string MessageTypeName { get { return EventBusManager.GetMessageExchangeName(MessageType); } }

        public Type MessageType { get; protected set; }
        
        public virtual Task OnMessage(EventBusMessage message)
        {
            return _onMessage(message);
        }
    }

    public class EventBusSubscription<TMessage> : EventBusSubscription where TMessage : EventBusMessage
    {
        public EventBusSubscription(Func<TMessage, Task> onMessage)
            : base(typeof(TMessage), (m) => onMessage((TMessage)m))
        {
        }
    }
}