using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CQRSSolution.Infrastructure.Persistence.Repositories;

/// <summary>
///     Provides an implementation of <see cref="IOrderRepository" /> using Entity Framework Core.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderRepository" /> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public OrderRepository(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    ///     Adds a new order to the database.
    ///     Note: In the current CreateOrderCommandHandler, the Order is added directly to the DbContext 
    ///     and SaveChangesAsync is called on the context. This AddAsync might be redundant if that pattern persists.
    ///     However, it's good practice for a repository to have an Add method.
    /// </summary>
    /// <param name="order">The order to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        // Typically, SaveChangesAsync would be called by a Unit of Work or the command handler
        // after all operations in a transaction are complete. Not here.
    }

    /// <summary>
    ///     Retrieves an order by its unique identifier, including its items.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The order if found, including items; otherwise, null.</returns>
    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking() // Good practice for read-only queries
            .Include(o => o.OrderItems) // Ensure OrderItems are loaded
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }

    // Future Order-specific methods would be implemented here.
    // For example:
    // public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    // {
    //     return await _dbSet.Where(o => o.CustomerId == customerId).ToListAsync(cancellationToken);
    // }
}