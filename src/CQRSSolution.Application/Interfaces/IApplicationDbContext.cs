using CQRSSolution.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// For DatabaseFacade

namespace CQRSSolution.Application.Interfaces;

/// <summary>
///     Interface for the application's database context, abstracting EF Core specific details from Application layer.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    ///     Gets the DbSet for Orders.
    /// </summary>
    DbSet<Order> Orders { get; }

    /// <summary>
    ///     Gets the DbSet for OrderItems.
    /// </summary>
    DbSet<OrderItem> OrderItems { get; }

    /// <summary>
    ///     Gets the DbSet for OutboxMessages.
    /// </summary>
    DbSet<OutboxMessage> OutboxMessages { get; }

    /// <summary>
    ///     Gets the DbSet for Customers.
    /// </summary>
    DbSet<Customer> Customers { get; }

    /// <summary>
    ///     Gets an <see cref="Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade" /> that can be used to ensure the
    ///     database is created and to manage transactions.
    /// </summary>
    DatabaseFacade Database { get; }

    /// <summary>
    ///     Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}