using CQRSSolution.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSSolution.Application.Interfaces;

/// <summary>
///     Defines the interface for the repository managing Order entities.
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    ///     Gets an order by its ID, including its associated order items.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The order if found, including its items; otherwise, null.</returns>
    Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new order to the repository.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Order order, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the order if found; otherwise, null.</returns>
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken);

    // Potentially other order-specific methods here, for example:
    // Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    // Task<IEnumerable<Order>> GetOrdersWithStatusAsync(string status, CancellationToken cancellationToken = default);
}