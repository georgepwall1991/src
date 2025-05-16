using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CQRSSolution.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
// For IConfiguration

namespace CQRSSolution.Infrastructure.BackgroundServices;
// Or a new CQRSSolution.Infrastructure.Messaging namespace

/// <summary>
///     Publishes events to Azure Service Bus.
/// </summary>
public class AzureServiceBusPublisher : IEventBusPublisher, IAsyncDisposable
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<AzureServiceBusPublisher> _logger;
    private readonly string? _queueName;
    private readonly ServiceBusClient? _serviceBusClient;
    private readonly ServiceBusSender? _serviceBusSender;

    public AzureServiceBusPublisher(IConfiguration configuration, ILogger<AzureServiceBusPublisher> logger,
        JsonSerializerOptions jsonSerializerOptions)
    {
        _logger = logger;
        _jsonSerializerOptions =
            jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        var connectionString = configuration["AzureServiceBus:ConnectionString"];
        _queueName = configuration["AzureServiceBus:QueueName"];

        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.Equals("YOUR_AZURE_SERVICE_BUS_CONNECTION_STRING", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "Azure Service Bus connection string is not configured or is set to the default placeholder. Event publishing will be skipped.");
            _serviceBusClient = null;
            _serviceBusSender = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(_queueName))
        {
            _logger.LogWarning(
                "Azure Service Bus queue name is not configured. Event publishing will be skipped for queue-based operations.");
            _serviceBusClient = null;
            _serviceBusSender = null;
            return;
        }

        _serviceBusClient = new ServiceBusClient(connectionString);
        _serviceBusSender = _serviceBusClient.CreateSender(_queueName);
        _logger.LogInformation("AzureServiceBusPublisher initialized for queue: {QueueName}", _queueName);
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceBusSender != null) await _serviceBusSender.DisposeAsync();
        if (_serviceBusClient != null) await _serviceBusClient.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public async Task PublishAsync(object eventData, string eventType, Guid messageId, CancellationToken cancellationToken = default)
    {
        if (_serviceBusSender == null || _serviceBusClient == null || string.IsNullOrWhiteSpace(_queueName))
        {
            _logger.LogWarning(
                "Azure Service Bus client, sender, or queue name not initialized (likely due to missing configuration). Skipping event publishing for event type {EventType}.",
                eventType);
            return;
        }

        try
        {
            var eventJson = JsonSerializer.Serialize(@eventData, _jsonSerializerOptions);
            var message = new ServiceBusMessage(eventJson)
            {
                ContentType = "application/json",
                Subject = eventType, // Using Subject to store the event type, useful for subscribers
                MessageId = Guid.NewGuid().ToString() // Ensure unique message ID
            };

            _logger.LogInformation(
                "Publishing event of type {EventType} to Azure Service Bus queue {QueueName}. MessageId: {MessageId}",
                eventType, _queueName, message.MessageId);
            await _serviceBusSender.SendMessageAsync(message, cancellationToken);
            _logger.LogInformation(
                "Successfully published event {EventType} with MessageId {MessageId} to queue {QueueName}.", eventType,
                message.MessageId, _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event type {EventType} to Azure Service Bus queue {QueueName}.",
                eventType, _queueName);
            throw; // Re-throw to allow OutboxProcessorService to handle retry/failure logic
        }
    }

    public Task PublishAsync(object eventData, Type eventType, string messageId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}