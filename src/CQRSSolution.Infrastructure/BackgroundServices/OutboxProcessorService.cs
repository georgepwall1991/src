using System.Text.Json;
using CQRSSolution.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;

namespace CQRSSolution.Infrastructure.BackgroundServices;

/// <summary>
///     Background service that periodically processes messages from the outbox.
/// </summary>
public class OutboxProcessorService : BackgroundService
{
    // Configuration for the outbox processor (can be moved to appsettings.json)
    private const int PollingIntervalSeconds = 10; // How often to poll
    private const int MaxMessagesToProcessPerCycle = 20; // Max messages to grab in one go
    private const int MaxRetryAttempts = 3; // Max times to retry a message before marking as failed

    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(PollingIntervalSeconds); // Configurable
    private readonly int _batchSize = MaxMessagesToProcessPerCycle; // Configurable
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        // Add any other necessary deserialization options here
    };

    public OutboxProcessorService(ILogger<OutboxProcessorService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// This method is called when the <see cref="IHostedService"/> starts.
    /// The implementation should return a task that represents the lifetime of the long running operation(s) being performed.
    /// </summary>
    /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(CancellationToken)"/> is called.</param>
    /// <returns>A <see cref="Task"/> that represents the long running operations.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service is starting.");

        stoppingToken.Register(() => _logger.LogInformation("Outbox Processor Service is stopping."));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in Outbox Processor Service execution loop.");
                // Consider a longer delay here if errors are persistent to avoid tight loop of failures
            }
            await Task.Delay(_pollingInterval, stoppingToken); 
        }
        _logger.LogInformation("Outbox Processor Service has stopped.");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Polling for outbox messages...");

        // Create a scope to resolve scoped services
        using var scope = _serviceProvider.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        var eventBusPublisher = scope.ServiceProvider.GetRequiredService<IEventBusPublisher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>(); // For SaveChangesAsync

        var messages = await outboxRepository.GetUnprocessedMessagesAsync(_batchSize, cancellationToken);

        if (!messages.Any())
        {
            _logger.LogDebug("No unprocessed outbox messages found.");
            return;
        }

        _logger.LogInformation("Found {MessageCount} unprocessed outbox messages to process.", messages.Count);

        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested) break;

            object? deserializedEvent = null;
            try
            {
                // Attempt to deserialize the event payload
                Type? eventType = FindType(message.Type);
                if (eventType == null)
                {
                    _logger.LogError("Could not find type {EventType} for outbox message {MessageId}. Skipping.", message.Type, message.Id);
                    message.Error = $"Type {message.Type} not found.";
                    // Optionally mark as processed if it's an unrecoverable type issue, or leave for manual inspection
                    // message.ProcessedOnUtc = DateTime.UtcNow; 
                }
                else
                {
                    deserializedEvent = JsonSerializer.Deserialize(message.Payload, eventType, _jsonSerializerOptions);
                    if (deserializedEvent == null)
                    {
                        _logger.LogError("Failed to deserialize payload for outbox message {MessageId} of type {EventType}. Payload: {Payload}", message.Id, message.Type, message.Payload);
                        message.Error = "Failed to deserialize payload.";
                    }
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON Deserialization error for outbox message {MessageId} of type {EventType}. Payload: {Payload}", message.Id, message.Type, message.Payload);
                message.Error = "JSON Deserialization error: " + jsonEx.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during deserialization for outbox message {MessageId} of type {EventType}.", message.Id, message.Type);
                message.Error = "Unexpected deserialization error: " + ex.Message;
            }

            if (deserializedEvent != null)
            {
                try
                {
                    _logger.LogInformation("Publishing event of type {EventType} from outbox message {MessageId}.", message.Type, message.Id);
                    await eventBusPublisher.PublishAsync(deserializedEvent, cancellationToken);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.Error = null; // Clear previous errors if any
                    _logger.LogInformation("Successfully published event from outbox message {MessageId} and marked as processed.", message.Id);
                }
                catch (Exception ex)
                {
                    // Handle transient or persistent errors from event bus publisher
                    _logger.LogError(ex, "Error publishing event from outbox message {MessageId} of type {EventType}.", message.Id, message.Type);
                    message.Error = "Publishing error: " + ex.Message;
                    // Implement retry logic or dead-lettering if necessary for the event bus publisher itself.
                    // For now, we'll update the message with the error and it will be retried later.
                }
            }
            
            // Update the outbox message (e.g., set ProcessedOnUtc or Error)
            await outboxRepository.UpdateAsync(message, cancellationToken);
        }
        
        // Save all changes made to outbox messages in this batch
        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Finished processing batch of {MessageCount} outbox messages.", messages.Count);
    }

    private static Type? FindType(string typeName)
    {
        // A more robust type resolution mechanism might be needed, 
        // especially if events are in different assemblies.
        // This simple version searches loaded assemblies.
        var type = Type.GetType(typeName);
        if (type != null) return type;

        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = a.GetType(typeName);
            if (type != null)
                return type;
        }
        return null;
    }
}

// Placeholder/Mock implementation for IEventBusPublisher
// This would typically be in its own file and could be an AzureServiceBusPublisher, RabbitMQPublisher, etc.
// This would typically be in its own file and could be an AzureServiceBusPublisher, RabbitMQPublisher, etc.