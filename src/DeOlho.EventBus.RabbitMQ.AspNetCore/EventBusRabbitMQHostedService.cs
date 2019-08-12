using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeOlho.EventBus.RabbitMQ.AspNetCore
{
    public class EventBusRabbitMQHostedService : IHostedService
    {
        readonly IMediator _mediator;
        readonly IEventBus _eventBus;
        public EventBusRabbitMQHostedService(IServiceCollection services, IMediator mediator, IEventBus eventBus)
        {
            _mediator = mediator;
            _eventBus = eventBus;

            var consumerTypes = services
                .Where(_ => _.ServiceType.IsGenericType 
                    && typeof(IRequestHandler<,>).IsAssignableFrom(_.ServiceType.GetGenericTypeDefinition())
                    && _.ServiceType.GenericTypeArguments[0].IsGenericType
                    && typeof(EventBusConsumer<>).IsAssignableFrom(_.ServiceType.GenericTypeArguments[0].GetGenericTypeDefinition()))
                .Select(_ => _.ServiceType)
                .ToList();

            var consumerFailTypes = services
                .Where(_ => _.ServiceType.IsGenericType 
                    && typeof(IRequestHandler<,>).IsAssignableFrom(_.ServiceType.GetGenericTypeDefinition())
                    && _.ServiceType.GenericTypeArguments[0].IsGenericType
                    && typeof(EventBusConsumerFail<>).IsAssignableFrom(_.ServiceType.GenericTypeArguments[0].GetGenericTypeDefinition()))
                .Select(_ => _.ServiceType)
                .ToList();

            foreach(var consumerType in consumerTypes)
            {
                var enventBusConsummerType = consumerType.GenericTypeArguments[0];
                var eventBusMessageType = enventBusConsummerType.GenericTypeArguments[0];
                _eventBus.Subscribe(eventBusMessageType, 
                    (c, m) => 
                    {
                        var eventBusConsumerType = typeof(EventBusConsumer<>).MakeGenericType(eventBusMessageType);
                        var eventBusConsumer = (IRequest)Activator.CreateInstance(eventBusConsumerType, m);
                        return mediator.Send(eventBusConsumer);
                    });
            }

            foreach(var consumerFailType in consumerFailTypes)
            {
                var enventBusConsummerType = consumerFailType.GenericTypeArguments[0];
                var eventBusMessageType = enventBusConsummerType.GenericTypeArguments[0];
                _eventBus.SubscribeFail(eventBusMessageType, 
                    (c, m) => 
                    {
                        var eventBusConsumerType = typeof(EventBusConsumerFail<>).MakeGenericType(eventBusMessageType);
                        var eventBusConsumer = (IRequest)Activator.CreateInstance(eventBusConsumerType, m, c.ExceptionMessageStack);
                        return mediator.Send(eventBusConsumer);
                    });
            }
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