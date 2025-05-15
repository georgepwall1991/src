using CQRSSolution.Application.Interfaces;
using CQRSSolution.Infrastructure.Persistence.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace CQRSSolution.Infrastructure.Persistence;

/// <summary>
///     Implements the <see cref="IUnitOfWork" /> interface using Entity Framework Core.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private const int RetryCount = 3;
    private readonly ApplicationDbContext _dbContext;
    private readonly AsyncRetryPolicy _dbSaveRetryPolicy;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnitOfWork" /> class.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="customerRepository">The customer repository.</param>
    /// <param name="orderRepository">The order repository.</param>
    /// <param name="outboxMessageRepository">The outbox message repository.</param>
    /// <param name="logger">The logger.</param>
    public UnitOfWork(
        ApplicationDbContext dbContext,
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IOutboxMessageRepository outboxMessageRepository,
        ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Customers = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        Orders = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        OutboxMessages = outboxMessageRepository ?? throw new ArgumentNullException(nameof(outboxMessageRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        
        _dbSaveRetryPolicy = Policy
            .Handle<SqlException>(IsTransientDbException)
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                RetryCount,
                attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (exception, timespan, attempt, context) =>
                {
                    _logger.LogWarning(exception, "Attempt {Attempt} to save changes to the database failed due to a transient error. Retrying in {Timespan}. Context: {Context}", attempt, timespan, context);
                });
    }

    /// <summary>
    ///     Gets the repository for Customer entities.
    /// </summary>
    public ICustomerRepository Customers { get; }

    /// <summary>
    ///     Gets the repository for Order entities.
    /// </summary>
    public IOrderRepository Orders { get; }

    /// <summary>
    ///     Gets the repository for OutboxMessage entities.
    /// </summary>
    public IOutboxMessageRepository OutboxMessages { get; }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
            throw new InvalidOperationException("A transaction is already in progress. Commit or roll back the existing transaction before starting a new one.");
        
        _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction == null) throw new InvalidOperationException("Transaction has not been started.");
            await _currentTransaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Transaction {TransactionId} committed successfully.", _currentTransaction.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction commit failed. Attempting to rollback. Transaction ID: {TransactionId}", _currentTransaction?.TransactionId);
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transactionId = _currentTransaction?.TransactionId;
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
                _logger.LogInformation("Transaction {TransactionId} rolled back successfully.", transactionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback transaction {TransactionId}.", transactionId);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <inheritdoc />
    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSaveRetryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Attempting to save changes to the database.");
            var result = await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("{Result} changes saved to the database successfully.", result);
            return result;
        });
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction != null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
            _logger.LogDebug("Disposed active transaction during UnitOfWork disposal.");
        }

        GC.SuppressFinalize(this);
    }

    private static bool IsTransientDbException(SqlException ex)
    {
        return Enum.IsDefined(typeof(SqlTransientErrorCodes), ex.Number);
    }
}