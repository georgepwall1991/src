using System.Text.Json;
using CQRSSolution.Domain.Entities;

namespace CQRSSolution.Application.Factories;

/// <summary>
///     Factory responsible for creating <see cref="OutboxMessage" /> instances.
/// </summary>
public class OutboxMessageFactory : IOutboxMessageFactory
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OutboxMessageFactory" /> class with specified serializer options.
    /// </summary>
    /// <param name="jsonSerializerOptions">The JSON serializer options to use.</param>
    public OutboxMessageFactory(JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions =
            jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }

    /// <summary>
    ///     Creates a new <see cref="OutboxMessage" /> from a domain event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event.</typeparam>
    /// <param name="domainEvent">The domain event to be stored in the outbox.</param>
    /// <returns>A new <see cref="OutboxMessage" /> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="domainEvent" /> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the full name of the event type cannot be determined.</exception>
    public OutboxMessage CreateFromDomainEvent<TEvent>(TEvent domainEvent) where TEvent : class
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));

        var eventTypeFullName = domainEvent.GetType().FullName;
        if (string.IsNullOrEmpty(eventTypeFullName))
            throw new InvalidOperationException(
                $"Could not determine the full name for event type {domainEvent.GetType().Name}.");

        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = eventTypeFullName,
            Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), _jsonSerializerOptions)
        };
    }
}

/// <summary>
///     Interface for the <see cref="OutboxMessageFactory" />.
/// </summary>
public interface IOutboxMessageFactory
{
    /// <summary>
    ///     Creates a new <see cref="OutboxMessage" /> from a domain event.
    /// </summary>
    /// <typeparam name="TEvent">The type of the domain event.</typeparam>
    /// <param name="domainEvent">The domain event to be stored in the outbox.</param>
    /// <returns>A new <see cref="OutboxMessage" /> instance.</returns>
    OutboxMessage CreateFromDomainEvent<TEvent>(TEvent domainEvent) where TEvent : class;
}