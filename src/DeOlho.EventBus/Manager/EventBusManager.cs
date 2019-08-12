using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeOlho.EventBus.Message;
using Microsoft.Extensions.Logging;

namespace DeOlho.EventBus.Manager
{
    public class EventBusManager
    {
        readonly EventBusConfiguration _configuration;
        readonly List<EventBusSubscription> _subscriptions;
        readonly ILogger<EventBusManager> _logger;

        public EventBusManager(
            EventBusConfiguration configuration,
            ILogger<EventBusManager> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _subscriptions = new List<EventBusSubscription>();
        }

        public IReadOnlyList<EventBusSubscription> Subscriptions => _subscriptions.AsReadOnly();

        public bool HasSubscription(string messageTypeName)
        {
            return _subscriptions.Any(_ => _.SubscriptionEventName == messageTypeName);
        }

        public EventBusSubscription GetSubscription(string subscriptionName)
        {
            return _subscriptions.SingleOrDefault(_ => _.SubscriptionEventName == subscriptionName);
        }

        public EventBusSubscription AddSubscription<TMessage>(Func<EventBusSubscriptionContext, TMessage, Task> onMessage, bool isSubscriptionForFail = false) where TMessage : EventBusMessage
        {
            var subscription = new EventBusSubscription<TMessage>(this, onMessage, isSubscriptionForFail);
            return AddSubscription(subscription);
        }

        public EventBusSubscription AddSubscription(Type messageType, Func<EventBusSubscriptionContext, EventBusMessage, Task> onMessage, bool isSubscriptionForFail = false)
        {
            var subscription = new EventBusSubscription(messageType, this, onMessage, isSubscriptionForFail);
            return AddSubscription(subscription);
        }

        private EventBusSubscription AddSubscription(EventBusSubscription subscription)
        {
            if (!HasSubscription(subscription.SubscriptionEventName))
            {
                _subscriptions.Add(subscription);
                return subscription;
            }
            else
            {
                _logger.LogWarning($"Já existe uma assinatura para {subscription.SubscriptionEventName}");
                return null;
            }
        }

        public void Clear()
        {
            _subscriptions.Clear();
        }


        public string GetEventName<TMessage>() where TMessage : EventBusMessage
        {
            return GetEventName(typeof(TMessage));
        }

        public string GetEventName(Type messageType, bool isForFailSubscription = false)
        {
            if (!typeof(EventBusMessage).IsAssignableFrom(messageType))
            {
                throw new ArgumentException(nameof(messageType));
            }
            else
            {
                return $"{messageType.FullName}{(isForFailSubscription ? _configuration.FailSuffix : "")}";
            }
        }

    }

    
}