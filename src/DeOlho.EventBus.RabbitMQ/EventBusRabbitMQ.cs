using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        readonly EventBusRabbitMQConfiguration _configuration;
        readonly EventBusRabbitMQConnection _eventBusConnection;
        readonly ILogger<EventBusRabbitMQ> _logger;
        readonly EventBusManager _manager;
        readonly EventBusRabbitMQRetryConsumerStrategy _retryConsumerStrategy;
        readonly string _queueName;
        readonly string _exchangeName;
        IModel _consumerChannel;
        int[] _retrySequence = new int[] { 1, 10 };
        int _retryCount = 5;

        public EventBusRabbitMQ(
            EventBusRabbitMQConfiguration configuration,
            EventBusRabbitMQConnection eventBusConnection,
            EventBusRabbitMQRetryConsumerStrategy retryConsumerStrategy,
            EventBusManager manager,
            ILogger<EventBusRabbitMQ> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _exchangeName = configuration.ExchangeName;
            _queueName = configuration.QueueName;
            _eventBusConnection = eventBusConnection ?? throw new ArgumentNullException(nameof(eventBusConnection));
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _retryConsumerStrategy = retryConsumerStrategy ?? throw new ArgumentNullException(nameof(retryConsumerStrategy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // private void Manager_OnEventRemoved(object sender, string eventName)
        // {
        //     _eventBusConnection.EnsureConnection();

        //     using (var channel = _eventBusConnection.CreateModel())
        //     {
        //         channel.QueueUnbind(
        //             _queueName,
        //             _exchangeName,
        //             eventName);

        //         if (_manager.Subscriptions.Count == 0)
        //         {
        //             _consumerChannel.Close();
        //         }
        //     }
        // }

        private RetryPolicy createRetryPolicy(string eventName, string messageId)
        {
            return RetryPolicy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    _logger.LogWarning(ex, $"Could not publish event: {eventName} ({messageId}) after {time.TotalSeconds:n1}s ({ex.Message})");
                });
        }

        public void Publish<TMessage>(TMessage message) where TMessage : EventBusMessage
        {
            Publish(typeof(TMessage), message);
        }

        public void Publish(Type messageType, EventBusMessage message)
        {
            _eventBusConnection.EnsureConnection();

            var eventName = _manager.GetEventName(messageType);

            _logger.LogTrace($"Creating RabbitMQ channel to publish event: {message.MessageId} ({eventName})");

            using (var channel = _eventBusConnection.CreateModel())
            {

                _logger.LogTrace($"Declaring RabbitMQ exchange to publish event: {message.MessageId}");

                var messageJson = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                createRetryPolicy(eventName, message.MessageId).Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2; // persistent

                    _logger.LogTrace($"Publishing event to RabbitMQ: {message.MessageId}");

                    channel.BasicPublish(
                        exchange: _exchangeName,
                        eventName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                });
            }
        }

        public void Subscribe(Type messageType, Func<EventBusSubscriptionContext, EventBusMessage, Task> onMessage)
        {
            Subscribe(_manager.AddSubscription(messageType, onMessage));
        }

        public void Subscribe<TMessage>(Func<EventBusSubscriptionContext, TMessage, Task> onMessage) where TMessage : EventBusMessage
        {
            Subscribe(_manager.AddSubscription<TMessage>(onMessage));
        }

        public void SubscribeFail(Type messageType, Func<EventBusSubscriptionContext, EventBusMessage, Task> onMessage)
        {
            Subscribe(_manager.AddSubscription(messageType, onMessage, true));
        }

        public void SubscribeFail<TMessage>(Func<EventBusSubscriptionContext, TMessage, Task> onMessage) where TMessage : EventBusMessage
        {
            Subscribe(_manager.AddSubscription<TMessage>(onMessage, true));
        }
        
        public void Subscribe(EventBusSubscription eventBusSubscription)
        {
            if (eventBusSubscription != null)
            {
                _logger.LogInformation($"Subscribing event {eventBusSubscription.SubscriptionEventName}");

                DoInternalSubscription(eventBusSubscription);
                StartBasicConsume();
            }
        }
        

        private void DoInternalSubscription(EventBusSubscription subscription)
        {

            _eventBusConnection.EnsureConnection();

            using (var channel = _eventBusConnection.CreateModel())
            {
                var args = new Dictionary<string, object>();

                channel.QueueBind(
                    queue: _queueName,
                    exchange: _exchangeName,
                    routingKey: subscription.SubscriptionEventName,
                    args);
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

            if (_consumerChannel == null && _manager.Subscriptions.Any())
            {
                _consumerChannel = CreateConsumerChannel();

                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += ConsumerReceived;

                _consumerChannel.BasicConsume(
                    queue: _queueName,
                    autoAck: false,
                    consumer: consumer);
            }
        }

        private async Task ConsumerReceived(object sender, BasicDeliverEventArgs eventArgs)
        {
            try
            {
                await _manager.ProcessEvent(eventArgs, _logger);
                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _retryConsumerStrategy.PublishRetry(_consumerChannel, eventArgs, ex);
                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            
        }

        private IModel CreateConsumerChannel()
        {
            _eventBusConnection.EnsureConnection();

            _logger.LogInformation("Creating RabbitMQ consumer channel");

            var channel = _eventBusConnection.CreateModel();

            channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object> { 
                    //{ "x-dead-letter-exchange" , _exchangeName } 
                });

            _retryConsumerStrategy.CreateExchangeAndQueueForRetryStrategy(channel, _exchangeName);

            channel.CallbackException += (sender, ea) =>
            {
                if (!(ea.Exception is InvalidOperationException))
                {
                    _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");
                    _consumerChannel.Dispose();
                    _consumerChannel = null;
                    StartBasicConsume();
                }
            };

            return channel;
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