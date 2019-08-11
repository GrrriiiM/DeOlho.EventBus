using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DeOlho.EventBus.RabbitMQ
{
    public class EventBusRabbitMQRetryConsumerStrategy
    {
        readonly EventBusConfiguration _configuration;
        readonly ILogger<EventBusRabbitMQRetryConsumerStrategy> _logger;
        readonly int[] _retryInterval = new int[] { 1, 10 };

        public EventBusRabbitMQRetryConsumerStrategy(
            EventBusConfiguration configuration,
            ILogger<EventBusRabbitMQRetryConsumerStrategy> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void CreateExchangeAndQueueForRetryStrategy(IModel channel, string exchangeName)
        {
            channel.ExchangeDeclare(
                exchange: exchangeName, 
                type: ExchangeType.Direct,
                arguments: new Dictionary<string, object>()
                {
                    { "x-dead-letter-exchange", $"{exchangeName}{_configuration.FailSuffix}"  }
                });

            channel.ExchangeDeclare(
                exchange: $"{exchangeName}{_configuration.FailSuffix}", 
                type: ExchangeType.Fanout,
                arguments: new Dictionary<string, object>()
                {
                });

            channel.ExchangeDeclare(
                exchange: $"{exchangeName}{_configuration.RetrySuffix}", 
                type: ExchangeType.Fanout,
                arguments: new Dictionary<string, object>()
                {
                    { "x-dead-letter-exchange", exchangeName  }
                });

            channel.QueueDeclare(
                queue: "retry-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object> { 
                    { "x-dead-letter-exchange" , exchangeName } 
                });

            channel.QueueBind(
                queue: "retry-queue",
                exchange: $"{exchangeName}{_configuration.FailSuffix}",
                "");

            channel.QueueDeclare(
                queue: "fail-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object> { 
                });

            channel.QueueBind(
                queue: "fail-queue",
                exchange: $"{exchangeName}{_configuration.FailSuffix}",
                "");
        }

        public void PublishRetry(IModel channel, BasicDeliverEventArgs eventArgs, Exception exception)
        {
            try
            {
                _logger.LogError(exception, $"Erro ao processar a mensagem {eventArgs.RoutingKey}: {exception.Message}\nNova tentativa em ");


                var retryCountName = "retry-count";
                var exceptionStackName = "exception-stack";
                var isFailMessageName = "is-fail-message";

                var properties = eventArgs.BasicProperties;
                properties.Headers = properties.Headers ?? new Dictionary<string, object>();

                if (!properties.Headers.ContainsKey(retryCountName)) properties.Headers.Add(retryCountName, 0);
                if (!properties.Headers.ContainsKey(exceptionStackName)) properties.Headers.Add(exceptionStackName, new ArrayList());
                
                var retryCount = (int)properties.Headers[retryCountName];
                IList exceptionStack = (IList)properties.Headers[exceptionStackName];
                
                retryCount += 1;
                exceptionStack.Insert(0, exception.Message);

                properties.Headers[exceptionStackName] = exceptionStack;

                if (retryCount <= _retryInterval.Length)
                {
                    properties.Headers[retryCountName] = retryCount;
                    properties.Expiration = $"{_retryInterval[retryCount - 1] * 1000}";

                    channel.BasicPublish(
                        exchange: $"{eventArgs.Exchange}-retry",
                        routingKey: eventArgs.RoutingKey,
                        mandatory: true,
                        basicProperties: properties,
                        body: eventArgs.Body);

                }
                else
                {
                    if (!properties.Headers.ContainsKey(isFailMessageName))
                    {
                        properties.Headers[retryCountName] = 0;
                        properties.Headers[isFailMessageName] = 1;
                        channel.BasicPublish(
                            exchange: $"{eventArgs.Exchange}",
                            routingKey: $"{eventArgs.RoutingKey}-fail",
                            mandatory: true,
                            basicProperties: properties,
                            body: eventArgs.Body);
                    }
                    else
                    {
                        properties.Headers[retryCountName] = 0;
                        properties.Headers[isFailMessageName] = 1;
                        channel.BasicPublish(
                            exchange: $"{eventArgs.Exchange}-fail",
                            routingKey: $"{eventArgs.RoutingKey}",
                            mandatory: true,
                            basicProperties: properties,
                            body: eventArgs.Body);
                    }

                    
                }
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }
    }
}