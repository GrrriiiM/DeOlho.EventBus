using System;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.Manager;
using DeOlho.EventBus.RabbitMQ;
using DeOlho.EventBus.RabbitMQ.DependencyInjection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DeOlho.EventBus
{
    public static class DependencyInjectionExtensions
    {

        public static IServiceCollection AddEventBusRabbitMQ(this IServiceCollection services, Action<EventBusRabbitMQDependencyInjectionConfiguration> config)
        {
            var c = new EventBusRabbitMQDependencyInjectionConfiguration();

            config(c);

            services.AddSingleton<EventBusConfiguration>(serviceProvider => 
            {
                var _ = new EventBusConfiguration();
                _.FailSuffix = c.FailSuffix ?? _.FailSuffix;
                _.RetrySuffix = c.RetrySuffix ?? _.RetrySuffix;
                return _;
            })
            .AddSingleton<EventBusRabbitMQConfiguration>(serviceProvider => 
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
            .AddSingleton<IEventBus>(serviceProvider => 
            {
                var eventBus = new EventBusRabbitMQ(
                    serviceProvider.GetService<EventBusConfiguration>(),
                    serviceProvider.GetService<EventBusRabbitMQConfiguration>(),
                    serviceProvider.GetService<EventBusRabbitMQConnection>(),
                    serviceProvider.GetService<EventBusRabbitMQRetryConsumerStrategy>(),
                    serviceProvider.GetService<EventBusManager>(),
                    serviceProvider.GetService<ILogger<EventBusRabbitMQ>>()
                );

                foreach(var subscribe in c._subscribes)
                {
                    subscribe.Value(eventBus, serviceProvider);
                }

                foreach(var subscribeFail in c._subscribesFail)
                {
                    subscribeFail.Value(eventBus, serviceProvider);
                }

                return eventBus;
            });
            
            return services;
        }
    }
}