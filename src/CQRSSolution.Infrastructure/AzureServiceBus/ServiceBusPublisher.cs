using Azure.Messaging.ServiceBus;
using CQRSSolution.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSSolution.Infrastructure.AzureServiceBus
{
    public class ServiceBusPublisherOptions
    {
        public string ConnectionString { get; set; }
        public string DefaultTopicOrQueueName { get; set; } // e.g., "ordersevents"
    }

    public class ServiceBusPublisher : IEventBusPublisher, IAsyncDisposable
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusPublisherOptions _options;
        private readonly ILogger<ServiceBusPublisher> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();

        public ServiceBusPublisher(
            IOptions<ServiceBusPublisherOptions> options,
            ILogger<ServiceBusPublisher> logger,
            JsonSerializerOptions jsonSerializerOptions)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options), "ServiceBusPublisherOptions cannot be null.");
            if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                throw new ArgumentException("Service Bus ConnectionString must be configured.", nameof(options));
            }
            _logger = logger;
            _serviceBusClient = new ServiceBusClient(_options.ConnectionString);
            _jsonSerializerOptions = jsonSerializerOptions;
        }

        public async Task PublishAsync(object eventData, string eventTypeFullName, Guid messageId, CancellationToken cancellationToken = default)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            if (string.IsNullOrWhiteSpace(eventTypeFullName)) throw new ArgumentNullException(nameof(eventTypeFullName));

            Type? eventType = Type.GetType(eventTypeFullName);
            if (eventType == null)
            {
                _logger.LogError("Could not resolve type {EventTypeFullName} during publishing.", eventTypeFullName);
                // Or throw a specific exception, or handle as a non-transient error upstream
                throw new ArgumentException($"Could not resolve type: {eventTypeFullName}", nameof(eventTypeFullName)); 
            }

            string topicOrQueueName = GetTopicOrQueueNameForEvent(eventType);
            ServiceBusSender sender = _senders.GetOrAdd(topicOrQueueName, (key) => _serviceBusClient.CreateSender(key));

            // Serialize with the specific event type for polymorphic serialization if eventData is a base type
            string eventPayloadJson = JsonSerializer.Serialize(eventData, eventType, _jsonSerializerOptions);
            var serviceBusMessage = new ServiceBusMessage(eventPayloadJson)
            {
                MessageId = messageId.ToString(),
                CorrelationId = messageId.ToString(),
                ContentType = "application/json",
                Subject = eventType.Name,
                ApplicationProperties =
                {
                    { "EventTypeFullName", eventTypeFullName } 
                }
            };

            try
            {
                _logger.LogInformation("Publishing event {EventTypeFullName} with MessageId {MessageId} to {TopicOrQueueName}.",
                    eventTypeFullName, serviceBusMessage.MessageId, topicOrQueueName);
                await sender.SendMessageAsync(serviceBusMessage, cancellationToken);
                _logger.LogInformation("Event {EventTypeFullName} with MessageId {MessageId} published successfully to {TopicOrQueueName}.",
                    eventTypeFullName, serviceBusMessage.MessageId, topicOrQueueName);
            }
            catch (ServiceBusException ex)
            {
                _logger.LogError(ex, "ServiceBusException publishing event {EventTypeFullName} with MessageId {MessageId} to {TopicOrQueueName}. IsTransient: {IsTransient}",
                    eventTypeFullName, serviceBusMessage.MessageId, topicOrQueueName, ex.IsTransient);
                throw;
            }
        }

        private string GetTopicOrQueueNameForEvent(Type eventType) // Stays with Type for internal logic
        {
            if (string.IsNullOrWhiteSpace(_options.DefaultTopicOrQueueName))
            {
                var generatedName = eventType.Name.ToLowerInvariant() + "s";
                _logger.LogWarning("ServiceBus:DefaultTopicOrQueueName is not configured. Falling back to generated name: {GeneratedName} for event type {EventType}. Configure explicitly for production.",
                    generatedName, eventType.FullName);
                return generatedName;
            }
            return _options.DefaultTopicOrQueueName;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var senderPair in _senders)
            {
                try
                {
                    await senderPair.Value.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing ServiceBusSender for {TopicOrQueueName}.", senderPair.Key);
                }
            }
            _senders.Clear();

            if (_serviceBusClient != null)
            {
                try
                {
                    await _serviceBusClient.DisposeAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing ServiceBusClient.");
                }
            }
            _logger.LogInformation("ServiceBusPublisher disposed.");
        }
    }
} 