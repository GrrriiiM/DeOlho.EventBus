using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.Manager;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace DeOlho.EventBus.RabbitMQ.AspNetCore
{
    public static class DependencyInjectionExtensions
    {

        public static IServiceCollection AddEventBusRabbitMQ(this IServiceCollection services, Action<EventBusRabbitMQSettings> configuration)
        {
            
            services.AddSingleton<IEventBus>(serviceProvider => 
            {

                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

                var config = new EventBusRabbitMQSettings();
                configuration(config);

                var connectionFactory = new ConnectionFactory();
                connectionFactory.HostName = config.HostName;
                connectionFactory.Port = config.Port;
                connectionFactory.UserName = config.UserName;
                connectionFactory.Password = config.Password;
                connectionFactory.VirtualHost = config.VirtualHost;

                var eventBusConnection = new EventBusConnectionRabbitMQ(connectionFactory, loggerFactory.CreateLogger<EventBusConnectionRabbitMQ>());

                var eventBusManager = new EventBusManager(loggerFactory.CreateLogger<EventBusManager>());

                var eventBus = new EventBusRabbitMQ(eventBusConnection, loggerFactory.CreateLogger<EventBusRabbitMQ>(), eventBusManager);

                foreach(var subscribe in config.Subscribes)
                {
                    subscribe.Value(eventBus);
                }

                // eventBus.Subscribe<MessageTest>(_ => 
                // {
                //     System.Diagnostics.Debug.WriteLine(_.Testando);
                //     return Task.CompletedTask;
                // });

                return eventBus;
            });

            
            return services;
        }
    }
}