using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CQRSSolution.Infrastructure.Persistence.Repositories;

/// <summary>
///     Provides an implementation of <see cref="IOrderRepository" /> using Entity Framework Core.
/// </summary>
public class OrderRepository : Repository<Order>, IOrderRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderRepository" /> class.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Orders
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