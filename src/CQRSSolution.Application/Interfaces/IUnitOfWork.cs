namespace CQRSSolution.Application.Interfaces;

/// <summary>
///     Defines the interface for a Unit of Work, which provides access to repositories
///     and manages transactions for a set of operations.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>
    ///     Gets the repository for Customer entities.
    /// </summary>
    ICustomerRepository Customers { get; }

    /// <summary>
    ///     Gets the repository for Order entities.
    /// </summary>
    IOrderRepository Orders { get; }

    /// <summary>
    ///     Gets the repository for OutboxMessage entities.
    /// </summary>
    IOutboxMessageRepository OutboxMessages { get; }

    /// <summary>
    ///     Begins a new database transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Commits the current database transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Rolls back the current database transaction.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves all changes made in this unit of work to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}