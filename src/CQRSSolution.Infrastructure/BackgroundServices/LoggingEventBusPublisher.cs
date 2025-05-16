using CQRSSolution.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CQRSSolution.Infrastructure.BackgroundServices;

public class LoggingEventBusPublisher : IEventBusPublisher
{
    private readonly ILogger<LoggingEventBusPublisher> _logger;

    public LoggingEventBusPublisher(ILogger<LoggingEventBusPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(object eventData, string eventTypeFullName, Guid messageId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Simulating event publication to event bus. Event Type: {EventType}, Event: {@Event}", eventTypeFullName, eventData);
        // In a real implementation, this would send the event to Azure Service Bus / RabbitMQ etc.
        return Task.CompletedTask;
    }
}