using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DeOlho.EventBus.Abstractions;
using DeOlho.EventBus.Manager;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace DeOlho.EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : IEventBus
    {
        readonly EventBusConnectionRabbitMQ _eventBusConnection;
        readonly ILogger<EventBusRabbitMQ> _logger;
        readonly EventBusManager _manager;
        readonly string _queueName;
        IModel _consumerChannel;
        int _retryCount = 5;

        public EventBusRabbitMQ(
            EventBusConnectionRabbitMQ eventBusConnection,
            ILogger<EventBusRabbitMQ> logger,
            EventBusManager manager)
        {
            _queueName = Assembly.GetEntryAssembly().GetName().Name;
            _eventBusConnection = eventBusConnection ?? throw new ArgumentNullException(nameof(eventBusConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _consumerChannel = CreateConsumerChannel();
        }

        private void Manager_OnEventRemoved(object sender, string eventName)
        {
            _eventBusConnection.EnsureConnection();

            using (var channel = _eventBusConnection.CreateModel())
            {
                channel.QueueUnbind(
                    _queueName,
                    eventName,
                    "");

                if (_manager.Subscriptions.Count == 0)
                {
                    _consumerChannel.Close();
                }
            }
        }

        private RetryPolicy createRetryPolicy(string messageTypeName, string messageId)
        {
            return RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, $"Could not publish event: {messageTypeName} ({messageId}) after {time.TotalSeconds:n1}s ({ex.Message})");
                });
        }

        public void Publish<TMessage>(TMessage message) where TMessage : EventBusMessage
        {
            Publish(typeof(TMessage), message);
        }

        public void Publish(Type messageType, EventBusMessage message)
        {
            _eventBusConnection.EnsureConnection();

            var exchangeName = EventBusManager.GetMessageExchangeName(messageType);

            _logger.LogTrace($"Creating RabbitMQ channel to publish event: {message.MessageId} ({exchangeName})");

            using (var channel = _eventBusConnection.CreateModel())
            {

                _logger.LogTrace($"Declaring RabbitMQ exchange to publish event: {message.MessageId}");

                channel.ExchangeDeclare(exchange: exchangeName, type: "direct");

                var messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                var deadLetterExchange = EventBusManager.GetMessageExchangeName(typeof(EventBusMessageFail<>).MakeGenericType(messageType));

                createRetryPolicy(exchangeName, message.MessageId).Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    _logger.LogTrace($"Publishing event to RabbitMQ: {message.MessageId}");

                    channel.BasicPublish(
                        exchangeName,
                        "",
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                });
            }
        }

        public void Subscribe(Type messageType, Func<EventBusMessage, Task> onMessage)
        {
            var exchangeName = EventBusManager.GetMessageExchangeName(messageType);

            _logger.LogInformation($"Subscribing event {exchangeName}");

            DoInternalSubscription(exchangeName);
            _manager.AddSubscription(messageType, onMessage);
            StartBasicConsume();
        }

        public void Subscribe<TMessage>(Func<TMessage, Task> onMessage) where TMessage : EventBusMessage
        {
            var exchangeName = EventBusManager.GetMessageExchangeName(typeof(TMessage));

            _logger.LogInformation($"Subscribing event {exchangeName}");

            DoInternalSubscription(exchangeName);
            _manager.AddSubscription<TMessage>(onMessage);
            StartBasicConsume();
        }

        

        private void DoInternalSubscription(string exchangeName)
        {
            var containsKey = _manager.HasSubscription(exchangeName);
            if (!containsKey)
            {
                _eventBusConnection.EnsureConnection();

                using (var channel = _eventBusConnection.CreateModel())
                {
                    channel.ExchangeDeclare(exchangeName, "direct");

                    channel.QueueBind(
                        _queueName,
                        exchangeName,
                        "");
                }
            }
        }

        public void Dispose()
        {
            if (_consumerChannel != null)
            {
                _consumerChannel.Dispose();
            }

            _manager.Clear();
        }

        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (_consumerChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_Received;

                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            try
            {
                var exchangeName = eventArgs.Exchange;
                var message = Encoding.UTF8.GetString(eventArgs.Body);

                await ProcessEvent(exchangeName, message);

                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"----- ERROR Processing message \"{ex.Message}\"");

                _consumerChannel.BasicReject(eventArgs.DeliveryTag, !eventArgs.Redelivered);
            }
            
        }

        private IModel CreateConsumerChannel()
        {
            _eventBusConnection.EnsureConnection();

            _logger.LogInformation("Creating RabbitMQ consumer channel");

            var channel = _eventBusConnection.CreateModel();

            channel.QueueDeclare(queue: _queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
            
            channel.CallbackException += (sender, ea) =>
            {
                if (!(ea.Exception is InvalidOperationException))
                {
                    _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");

                    _consumerChannel.Dispose();
                    _consumerChannel = CreateConsumerChannel();
                    StartBasicConsume();
                }
            };

            return channel;
        }

        private async Task ProcessEvent(string exchangeName, string messageJson)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", exchangeName);

            if (_manager.HasSubscription(exchangeName))
            {
                
                var subscription = _manager.GetSubscription(exchangeName);
                var message = (EventBusMessage)JsonConvert.DeserializeObject(messageJson, subscription.MessageType);
                await subscription.OnMessage(message);
            }
            else
            {
                _logger.LogWarning($"No subscription for RabbitMQ event: {exchangeName}");
            }
        }

        public void Unsubscribe<TMessage>() where TMessage : EventBusMessage
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(Type message)
        {
            throw new NotImplementedException();
        }
    }
}