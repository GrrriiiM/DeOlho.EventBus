using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeOlho.EventBus.Manager;
using DeOlho.EventBus.Message;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
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
                await subscription.OnMessage(message, eventArgs.BasicProperties.GetExceptionStack().OfType<string>().ToArray());
            }
            else
            {
                logger.LogWarning($"No subscription for RabbitMQ event: {eventName}");
            }
        }

        public static int GetRetryCount(this IBasicProperties properties)
        {
            properties.Headers = properties.Headers ?? new Dictionary<string, object>();
            if (properties.Headers.ContainsKey("retry-count") && int.TryParse(properties.Headers["retry-count"].ToString(), out int retryCount))
            {
                return retryCount;
            }
            else
            {
                return 0;
            }
        }

        public static void SetRetryCount(this IBasicProperties properties, int retryCount)
        {
            properties.Headers = properties.Headers ?? new Dictionary<string, object>();
            if (properties.Headers.ContainsKey("retry-count"))
            {
                properties.Headers["retry-count"] = retryCount;
            }
            else
            {
                properties.Headers.Add("retry-count", retryCount);
            }
        }

        public static List<string> GetExceptionStack(this IBasicProperties properties)
        {
            if (properties.Headers.ContainsKey("exception-stack") && properties.Headers["exception-stack"] is IList)
            {
                var exceptionStack = new List<string>();
                var list  = properties.Headers["exception-stack"] as IList;
                foreach(var item in list)
                {
                    if (item is string) exceptionStack.Add((string)item);
                    if (item is byte[]) exceptionStack.Add(System.Text.Encoding.UTF8.GetString((byte[])item));
                }
                return exceptionStack;
            }
            else
            {
                return new List<string>();
            }
        }

        public static void InsertExceptionStack(this IBasicProperties properties, string exceptionMessage)
        {
            properties.Headers = properties.Headers ?? new Dictionary<string, object>();
            var exceptionStack = GetExceptionStack(properties);
            exceptionStack.Insert(0, exceptionMessage);
            if (properties.Headers.ContainsKey("exception-stack"))
            {
                properties.Headers["exception-stack"] = exceptionStack;
            }
            else
            {
                properties.Headers.Add("exception-stack", exceptionStack);
            }
        }

        public static bool GetIsFailMessage(this IBasicProperties properties)
        {
            properties.Headers = properties.Headers ?? new Dictionary<string, object>();
            if (properties.Headers.ContainsKey("is-fail-message") && bool.TryParse(properties.Headers["retry-count"].ToString(), out bool isFailMessage))
            {
                return isFailMessage;
            }
            else
            {
                return false;
            }
        }

        public static void SetIsFailMessage(this IBasicProperties properties, bool isFailMessage)
        {
            properties.Headers = properties.Headers ?? new Dictionary<string, object>();
            if (properties.Headers.ContainsKey("is-fail-message"))
            {
                properties.Headers["is-fail-message"] = isFailMessage;
            }
            else
            {
                properties.Headers.Add("is-fail-message", isFailMessage);
            }
        }
    }
}