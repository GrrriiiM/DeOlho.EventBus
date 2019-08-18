using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.Manager;
using DeOlho.EventBus.MediatR;
using DeOlho.EventBus.Message;
using MediatR;

namespace DeOlho.EventBus.RabbitMQ.DependencyInjection
{
    public class EventBusRabbitMQDependencyInjectionConfiguration
    {

        internal Dictionary<Type, Action<IEventBus, IMediator>> _subscribes = new Dictionary<Type, Action<IEventBus, IMediator>>();

        internal Dictionary<Type, Action<IEventBus, IMediator>> _subscribesFail = new Dictionary<Type, Action<IEventBus, IMediator>>();


        public void Subscribe<TMessage>(Func<EventBusSubscriptionContext, TMessage, Task> onMessage) where TMessage : EventBusMessage
        {
            _subscribes.Add(typeof(TMessage),  (e,m) => e.Subscribe<TMessage>(onMessage));
        }

        public void Subscribe(Type messageType, Func<EventBusSubscriptionContext, EventBusMessage, Task> onMessage)
        {
            _subscribes.Add(messageType,  (e,m) => e.Subscribe(messageType, onMessage));
        }


        public void SubscribeFail<TMessage>(Func<EventBusSubscriptionContext, TMessage, Task> onMessage) where TMessage : EventBusMessage
        {
            _subscribesFail.Add(typeof(TMessage),  (e,m) => e.Subscribe<TMessage>(onMessage));
        }

        public void SubscribeFail(Type messageType, Func<EventBusSubscriptionContext, EventBusMessage, Task> onMessage)
        {
            _subscribesFail.Add(messageType,  (e,m) => e.Subscribe(messageType, onMessage));
        }
        

        public void SubscribeMediatorConsumers(params Assembly[] assemblies)
        {
            var consumerTypes = assemblies.SelectMany(_ => _.GetTypes())
                .Where(_ => _.BaseType != null 
                    && _.BaseType.IsGenericType 
                    && typeof(EventBusConsumerHandler<>).IsAssignableFrom(_.BaseType.GetGenericTypeDefinition()))
                .Select(_ => _.BaseType.GenericTypeArguments[0])
                .ToList();

            var consumerFailTypes = assemblies.SelectMany(_ => _.GetTypes())
                .Where(_ => _.BaseType != null 
                    && _.BaseType.IsGenericType 
                    && typeof(EventBusConsumerFailHandler<>).IsAssignableFrom(_.BaseType.GetGenericTypeDefinition()))
                .Select(_ => _.BaseType.GenericTypeArguments[0])
                .ToList();

            foreach(var consumerType in consumerTypes)
            {
                _subscribes.Add(consumerType,  (e,m) => e.SubscribeWithMediatorConsumer(consumerType, m));
            }

            foreach(var consumerFailType in consumerFailTypes)
            {
                _subscribesFail.Add(consumerFailType,  (e,m) => e.SubscribeFailWithMediatorConsumer(consumerFailType, m));
            }
        }

        public string HostName { get; set; }
        public int? Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }
        public string RetrySuffix { get; set; }
        public string FailSuffix { get; set; }
    }
}