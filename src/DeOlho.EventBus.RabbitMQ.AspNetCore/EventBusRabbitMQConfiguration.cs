using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;

namespace DeOlho.EventBus.RabbitMQ.AspNetCore
{
    public class EventBusRabbitMQSettings
    {

        Dictionary<Type, Action<IEventBus>> _subscribes = new Dictionary<Type, Action<IEventBus>>();
        public IReadOnlyDictionary<Type, Action<IEventBus>> Subscribes => _subscribes;

        public string HostName { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string QueueName { get; set; }

        public EventBusRabbitMQSettings()
        {
            QueueName = Assembly.GetEntryAssembly().GetName().Name;
            VirtualHost = "/";
        }

        public void AddSubscribe<TEventBusMessage>(Func<TEventBusMessage, Task> action) where TEventBusMessage : EventBusMessage
        {
            if (_subscribes.ContainsKey(typeof(TEventBusMessage)))
                throw new InvalidOperationException("Subscripe já registrado");

            _subscribes.Add(typeof(TEventBusMessage), eventBus => eventBus.Subscribe<TEventBusMessage>(action));
        }


    }
}