using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSSolution.Application.Interfaces
{
    /// <summary>
    /// Interface for publishing events to a message bus.
    /// </summary>
    public interface IEventBusPublisher
    {
        /// <summary>
        /// Publishes an event to the message bus.
        /// </summary>
        /// <typeparam name="TEvent">The type of the event to publish.</typeparam>
        /// <param name="event">The event object to publish.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous publishing operation.</returns>
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : class;
    }
} 