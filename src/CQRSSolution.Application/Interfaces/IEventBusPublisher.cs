using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSSolution.Application.Interfaces
{
    public interface IEventBusPublisher
    {
        /// <summary>
        /// Publishes an event to the event bus.
        /// </summary>
        /// <param name="eventData">The event data object.</param>
        /// <param name="eventTypeFullName">The full name of the event type.</param>
        /// <param name="messageId">A unique message ID, typically the OutboxMessage.Id, used for bus-level deduplication and correlation.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task PublishAsync(object eventData, string eventTypeFullName, Guid messageId, CancellationToken cancellationToken = default);
    }
} 