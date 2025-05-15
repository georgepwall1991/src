using System.Text.Json;
using CQRSSolution.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
// For IEventBusPublisher
// For IServiceScopeFactory
// For IHostedService
// For ILogger

namespace CQRSSolution.Infrastructure.BackgroundServices;

/// <summary>
///     Background service that periodically processes messages from the outbox.
/// </summary>
public class OutboxProcessorService : IHostedService, IDisposable
{
    // Configuration for the outbox processor (can be moved to appsettings.json)
    private const int PollingIntervalSeconds = 10; // How often to poll
    private const int MaxMessagesToProcessPerCycle = 20; // Max messages to grab in one go
    private const int MaxRetryAttempts = 3; // Max times to retry a message before marking as failed

    private readonly IEventBusPublisher _eventBusPublisher;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        // Add any other necessary deserialization options here
    };

    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private Timer? _timer;

    public OutboxProcessorService(ILogger<OutboxProcessorService> logger, IServiceScopeFactory scopeFactory,
        IEventBusPublisher eventBusPublisher)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _eventBusPublisher = eventBusPublisher;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Outbox Processor Service is starting.");
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(PollingIntervalSeconds));
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Outbox Processor Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _logger.LogInformation("Outbox Processor Service is working. Polling for messages...");
        ProcessOutboxMessagesAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessorService is processing messages.");

        using var scope = _scopeFactory.CreateScope();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<IApplicationDbContext>(); // Changed from ApplicationDbContext
        var domainEventDeserializer = scope.ServiceProvider.GetRequiredService<DomainEventDeserializer>();

        var messagesToProcess = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.Error == null) // Added Error check
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20) // Process in batches
            .ToListAsync(stoppingToken);

        foreach (var message in messagesToProcess)
        {
            _logger.LogInformation("Processing outbox message {MessageId} of type {MessageType}.", message.Id,
                message.Type);
            try
            {
                var domainEvent = domainEventDeserializer.Deserialize(message.Type, message.Payload);

                if (domainEvent == null)
                {
                    _logger.LogError(
                        "Could not deserialize event type {EventType} for OutboxMessage {OutboxMessageId}. Payload: {Payload}",
                        message.Type, message.Id, message.Payload);
                    message.Error = "Deserialization failed"; // Mark as error
                    message.ProcessedOnUtc = DateTime.UtcNow; // Consider it processed to avoid retrying indefinitely 
                    await dbContext.SaveChangesAsync(stoppingToken);
                    continue;
                }

                await _eventBusPublisher.PublishAsync(domainEvent, message.Type, stoppingToken);
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null; // Clear any previous error
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox message {MessageId} of type {MessageType}.", message.Id,
                    message.Type);
                message.Error = ex.Message; // Store error message
                // Implement retry logic here if desired before marking as permanently failed
                // For simplicity, we'll just mark with error for now. A retry count could be added to OutboxMessage.
            }
            finally
            {
                // Always save changes to the message (ProcessedOnUtc or Error)
                // This needs to be outside a transaction that might roll back if PublishAsync fails but we still want to record the attempt/error.
                // However, if PublishAsync itself is part of a larger distributed transaction, this logic would be more complex.
                // For a simple outbox, updating the message state after attempting to publish is common.
                try
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx,
                        "Failed to save state changes for outbox message {MessageId} after processing attempt.",
                        message.Id);
                }
            }
        }
    }
}

// Placeholder/Mock implementation for IEventBusPublisher
// This would typically be in its own file and could be an AzureServiceBusPublisher, RabbitMQPublisher, etc.