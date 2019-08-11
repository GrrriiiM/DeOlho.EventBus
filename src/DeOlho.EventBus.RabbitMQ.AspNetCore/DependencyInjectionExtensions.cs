using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.Manager;
using DeOlho.EventBus.RabbitMQ.AspNetCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DeOlho.EventBus.RabbitMQ
{
    public static class DependencyInjectionExtensions
    {

        public static IServiceCollection AddEventBusRabbitMQ(this IServiceCollection services, Action<EventBusRabbitMQAspNetCoreConfiguration> config)
        {
            var c = new EventBusRabbitMQAspNetCoreConfiguration();

            config(c);

            services.AddSingleton<EventBusConfiguration>(ServiceProvider => 
            {
                var _ = new EventBusConfiguration();
                _.FailSuffix = c.FailSuffix ?? _.FailSuffix;
                _.RetrySuffix = c.RetrySuffix ?? _.RetrySuffix;
                return _;
            })
            .AddSingleton<EventBusRabbitMQConfiguration>(ServiceProvider => 
            {
                var _ = new EventBusRabbitMQConfiguration();
                _.HostName = c.HostName ?? throw new ArgumentNullException(nameof(c.HostName));
                _.Port = c.Port ?? throw new ArgumentNullException(nameof(c.Port));
                _.UserName = c.UserName ?? throw new ArgumentNullException(nameof(c.UserName));
                _.Password = c.Password ?? throw new ArgumentNullException(nameof(c.Password));
                _.VirtualHost = c.VirtualHost ?? _.VirtualHost;
                _.ExchangeName = c.ExchangeName ?? _.ExchangeName;
                _.QueueName = c.QueueName ?? _.QueueName;
                return _;
            })
            .AddSingleton<EventBusManager>()
            .AddSingleton<IConnectionFactory, EventBusRabbitMQConnectionFactory>()
            .AddSingleton<EventBusRabbitMQConnection>()
            .AddSingleton<EventBusRabbitMQRetryConsumerStrategy>()
            .AddSingleton<IEventBus, EventBusRabbitMQ>();
            
            return services;
        }
    }
}