using System;
using RabbitMQ.Client;

namespace DeOlho.EventBus.RabbitMQ
{
    public class EventBusRabbitMQConnectionFactory : ConnectionFactory
    {
        readonly EventBusRabbitMQConfiguration _configuration;
        public EventBusRabbitMQConnectionFactory(
            EventBusRabbitMQConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            HostName = _configuration.HostName;
            Port = _configuration.Port;
            VirtualHost = _configuration.VirtualHost;
            UserName = _configuration.UserName;
            Password = _configuration.Password;
            DispatchConsumersAsync = true;
        }
    }
}