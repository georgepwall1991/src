using CQRSSolution.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSSolution.Application.Interfaces;

/// <summary>
///     Repository interface for OutboxMessage specific operations.
/// </summary>
public interface IOutboxMessageRepository
{
    /// <summary>
    ///     Adds a new outbox message to the repository.
    /// </summary>
    /// <param name="outboxMessage">The outbox message to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieves a batch of unprocessed outbox messages.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of unprocessed outbox messages.</returns>
    Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken);

    /// <summary>
    ///     Updates an outbox message, typically to mark it as processed or to record an error.
    /// </summary>
    /// <param name="outboxMessage">The outbox message to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken);

    // Potentially a method to remove processed messages after a certain period if needed.
    // Task DeleteProcessedMessagesAsync(DateTime olderThan, CancellationToken cancellationToken);
}