using CQRSSolution.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Threading;
using System.Threading.Tasks;

// For DatabaseFacade

namespace CQRSSolution.Application.Interfaces;

/// <summary>
/// Interface for the application's database context.
/// Provides access to entity sets and database operations.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets or sets the DbSet for Orders.
    /// </summary>
    DbSet<Order> Orders { get; set; }

    /// <summary>
    /// Gets or sets the DbSet for OrderItems.
    /// </summary>
    DbSet<OrderItem> OrderItems { get; set; }

    /// <summary>
    /// Gets or sets the DbSet for OutboxMessages.
    /// </summary>
    DbSet<OutboxMessage> OutboxMessages { get; set; }

    /// <summary>
    ///     Gets the DbSet for Customers.
    /// </summary>
    DbSet<Customer> Customers { get; }

    /// <summary>
    /// Gets the database facade for this context.
    /// Provides access to database-related operations like transactions.
    /// </summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}