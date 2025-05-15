using System.Text.Json;
using CQRSSolution.Domain.DomainEvents;
using Microsoft.Extensions.Logging;
// To access a known type in the domain events assembly

namespace CQRSSolution.Infrastructure.BackgroundServices;

public class DomainEventDeserializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<DomainEventDeserializer> _logger;

    public DomainEventDeserializer(ILogger<DomainEventDeserializer> logger, JsonSerializerOptions jsonSerializerOptions)
    {
        _logger = logger;
        _jsonSerializerOptions =
            jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }

    public object? Deserialize(string eventTypeFullName, string payload)
    {
        try
        {
            var domainAssembly = typeof(OrderCreatedDomainEvent).Assembly;
            var eventType = domainAssembly.GetType(eventTypeFullName) ?? Type.GetType(eventTypeFullName);

            if (eventType != null) return JsonSerializer.Deserialize(payload, eventType, _jsonSerializerOptions);

            _logger.LogError("Could not find event type: {EventTypeFullName}", eventTypeFullName);
            return null;
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON Deserialization error for type {EventTypeFullName}. Payload: {Payload}",
                eventTypeFullName, payload);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generic Deserialization error for type {EventTypeFullName}. Payload: {Payload}",
                eventTypeFullName, payload);
            return null;
        }
    }
}