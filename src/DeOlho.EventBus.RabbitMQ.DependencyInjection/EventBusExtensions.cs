using System;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.MediatR;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DeOlho.EventBus.RabbitMQ.DependencyInjection
{
    public static class EventBusExtensions
    {
        public static void SubscribeWithMediatorConsumer(this IEventBus eventBus, Type messageType, IServiceProvider serviceProvider)
        {
            eventBus.Subscribe(messageType, 
                async (c, m) => 
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetService<IMediator>();
                        var eventBusConsumerType = typeof(EventBusConsumer<>).MakeGenericType(messageType);
                        var eventBusConsumer = (IRequest)Activator.CreateInstance(eventBusConsumerType, m);
                        await mediator.Send(eventBusConsumer);
                    }
                });
        }

        public static void SubscribeFailWithMediatorConsumer(this IEventBus eventBus, Type messageType, IServiceProvider serviceProvider)
        {
            eventBus.SubscribeFail(messageType, 
                async (c, m) => 
                {
                    using (var scope = serviceProvider.CreateScope())
                    {
                        var mediator = scope.ServiceProvider.GetService<IMediator>();
                        var eventBusConsumerType = typeof(EventBusConsumerFail<>).MakeGenericType(messageType);
                        var eventBusConsumer = (IRequest)Activator.CreateInstance(eventBusConsumerType, m, c.ExceptionMessageStack);
                        await mediator.Send(eventBusConsumer);
                    }
                });
        }
    }
}