using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSSolution.Infrastructure.Persistence.Repositories;

/// <summary>
///     Provides an implementation of <see cref="IOutboxMessageRepository" /> using Entity Framework Core.
/// </summary>
public class OutboxMessageRepository : IOutboxMessageRepository
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OutboxMessageRepository" /> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public OutboxMessageRepository(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Adds a new outbox message to the DbContext.
    /// </summary>
    /// <param name="outboxMessage">The outbox message to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task AddAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        await _context.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
        // SaveChangesAsync will be called by the command handler as part of the transaction.
    }

    /// <summary>
    /// Retrieves a batch of unprocessed outbox messages, ordered by occurrence date.
    /// </summary>
    /// <param name="batchSize">The maximum number of messages to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of unprocessed outbox messages.</returns>
    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken)
    {
        return await _context.OutboxMessages
            .Where(om => om.ProcessedOnUtc == null)
            .OrderBy(om => om.OccurredOnUtc)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates an outbox message in the DbContext. 
    /// This is typically used to mark a message as processed or to record an error.
    /// </summary>
    /// <param name="outboxMessage">The outbox message to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public Task UpdateAsync(OutboxMessage outboxMessage, CancellationToken cancellationToken)
    {
        // EF Core tracks changes to entities. When SaveChangesAsync is called on the context 
        // (e.g., by the OutboxProcessorService after processing), the changes to this outboxMessage will be persisted.
        _context.OutboxMessages.Update(outboxMessage);
        return Task.CompletedTask; // Update itself is synchronous for EF Core change tracker.
    }
}