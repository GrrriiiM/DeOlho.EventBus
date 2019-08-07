using System;
using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static DeOlho.EventBus.RabbitMQ.AspNetCore.DependencyInjectionExtensions;

namespace DeOlho.EventBus.RabbitMQ.AspNetCore
{
    public class EventBusRabbitMQHostedService : IHostedService
    {
        readonly IServiceProvider _serviceProvider;
        readonly IEventBus _eventBus;
        public EventBusRabbitMQHostedService(IServiceProvider serviceProvider, IEventBus eventBus)
        {
            _serviceProvider = serviceProvider;
            _eventBus = eventBus;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}