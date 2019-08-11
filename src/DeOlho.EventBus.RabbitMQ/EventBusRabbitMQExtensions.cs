using System.Text;
using System.Threading.Tasks;
using DeOlho.EventBus.Manager;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;

namespace DeOlho.EventBus
{
    public static class EventBusRabbitMQExtensions
    {
        public async static Task ProcessEvent(this EventBusManager manager, BasicDeliverEventArgs eventArgs, ILogger logger)
        {
            var eventName = eventArgs.RoutingKey;
            var messageBody = Encoding.UTF8.GetString(eventArgs.Body);

            logger.LogTrace($"Processing RabbitMQ event: {eventName}", eventName);

            if (manager.HasSubscription(eventName))
            {
                var subscription = manager.GetSubscription(eventName);
                var message = (EventBusMessage)JsonConvert.DeserializeObject(messageBody, subscription.MessageType);
                await subscription.OnMessage(message);
            }
            else
            {
                logger.LogWarning($"No subscription for RabbitMQ event: {eventName}");
            }
        }
    }
}