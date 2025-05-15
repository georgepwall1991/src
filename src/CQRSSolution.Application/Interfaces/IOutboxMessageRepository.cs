using CQRSSolution.Domain.Entities;

namespace CQRSSolution.Application.Interfaces;

/// <summary>
///     Repository interface for OutboxMessage specific operations.
/// </summary>
public interface IOutboxMessageRepository : IRepository<OutboxMessage>
{
    /// <summary>
    ///     Gets a specified number of unprocessed outbox messages.
    /// </summary>
    /// <param name="count">The maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A list of unprocessed outbox messages.</returns>
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int count, CancellationToken cancellationToken = default);
}