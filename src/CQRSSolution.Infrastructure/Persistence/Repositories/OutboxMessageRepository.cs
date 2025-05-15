using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CQRSSolution.Infrastructure.Persistence.Repositories;

/// <summary>
///     Provides an implementation of <see cref="IOutboxMessageRepository" /> using Entity Framework Core.
/// </summary>
public class OutboxMessageRepository : Repository<OutboxMessage>, IOutboxMessageRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OutboxMessageRepository" /> class.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public OutboxMessageRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<List<OutboxMessage>> GetUnprocessedMessagesAsync(int count,
        CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            // Or throw ArgumentOutOfRangeException, depending on desired contract
            return new List<OutboxMessage>();

        return await _dbSet
            .Where(om => om.ProcessedOnUtc == null)
            .OrderBy(om => om.OccurredOnUtc)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}