using System;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.MediatR;
using MediatR;

namespace DeOlho.EventBus.RabbitMQ.DependencyInjection
{
    public static class EventBusExtensions
    {
        public static void SubscribeWithMediatorConsumer(this IEventBus eventBus, Type messageType, IMediator mediator)
        {
            eventBus.Subscribe(messageType, 
                (c, m) => 
                {
                    var eventBusConsumerType = typeof(EventBusConsumer<>).MakeGenericType(messageType);
                    var eventBusConsumer = (IRequest)Activator.CreateInstance(eventBusConsumerType, m);
                    return mediator.Send(eventBusConsumer);
                });
        }

        public static void SubscribeFailWithMediatorConsumer(this IEventBus eventBus, Type messageType, IMediator mediator)
        {
            eventBus.SubscribeFail(messageType, 
                (c, m) => 
                {
                    var eventBusConsumerType = typeof(EventBusConsumerFail<>).MakeGenericType(messageType);
                    var eventBusConsumer = (IRequest)Activator.CreateInstance(eventBusConsumerType, m, c.ExceptionMessageStack);
                    return mediator.Send(eventBusConsumer);
                });
        }
    }
}