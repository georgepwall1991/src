namespace CQRSSolution.Application.Interfaces;

/// <summary>
///     Defines the contract for an event bus publisher.
/// </summary>
public interface IEventBusPublisher
{
    /// <summary>
    ///     Publishes an event to the event bus.
    /// </summary>
    /// <param name="event">The event object to publish.</param>
    /// <param name="eventType">The fully qualified name of the event type, used for routing or topic selection.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PublishAsync(object @event, string eventType, CancellationToken cancellationToken = default);
}