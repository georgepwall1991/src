using CQRSSolution.Domain.Entities;

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

    // Potentially other order-specific methods here, for example:
    // Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    // Task<IEnumerable<Order>> GetOrdersWithStatusAsync(string status, CancellationToken cancellationToken = default);
}