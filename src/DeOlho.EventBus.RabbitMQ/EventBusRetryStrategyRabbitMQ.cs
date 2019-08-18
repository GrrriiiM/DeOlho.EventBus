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
                exchange: $"{exchangeName}{_configuration.RetrySuffix}",
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
                var properties = eventArgs.BasicProperties;
                var retryCount = properties.GetRetryCount();
                
                retryCount += 1;
                properties.InsertExceptionStack(exception.ToString());

                if (retryCount <= _configuration.ConsumerRetryInterval.Length)
                {
                    properties.SetRetryCount(retryCount);
                    var retryInterval = _configuration.ConsumerRetryInterval[retryCount - 1];
                    properties.Expiration = $"{retryInterval * 1000}";

                    _logger.LogError(exception, $"Erro ao processar a mensagem {eventArgs.RoutingKey}: {exception.Message}\nNova tentativa em {(retryInterval)}s");

                    channel.BasicPublish(
                        exchange: $"{eventArgs.Exchange}{_configuration.RetrySuffix}",
                        routingKey: eventArgs.RoutingKey,
                        mandatory: true,
                        basicProperties: properties,
                        body: eventArgs.Body);

                }
                else
                {
                    
                    if (!properties.GetIsFailMessage())
                    {
                        _logger.LogError(exception, $"Erro ao processar a mensagem {eventArgs.RoutingKey}: {exception.Message}\nLimite de tentativas excedidos, tentando compensação");
                        properties.SetRetryCount(0);
                        properties.SetIsFailMessage(true);
                        channel.BasicPublish(
                            exchange: $"{eventArgs.Exchange}",
                            routingKey: $"{eventArgs.RoutingKey}{_configuration.FailSuffix}",
                            mandatory: true,
                            basicProperties: properties,
                            body: eventArgs.Body);
                    }
                    else
                    {
                        _logger.LogCritical(exception, $"Erro ao processar a mensagem {eventArgs.RoutingKey}: {exception.Message}\nMensagem perdida");
                        properties.SetRetryCount(0);
                        channel.BasicPublish(
                            exchange: $"{eventArgs.Exchange}{_configuration.FailSuffix}",
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