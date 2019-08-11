using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;

namespace DeOlho.EventBus.RabbitMQ.AspNetCore
{
    public class EventBusRabbitMQAspNetCoreConfiguration
    {

        Dictionary<Type, Action<IEventBus>> _subscribes = new Dictionary<Type, Action<IEventBus>>();
        public IReadOnlyDictionary<Type, Action<IEventBus>> Subscribes => _subscribes;

        public string HostName { get; set; }
        public int? Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }
        public string RetrySuffix { get; set; }
        public string FailSuffix { get; set; }

        public EventBusRabbitMQAspNetCoreConfiguration()
        {
        }
    }
}