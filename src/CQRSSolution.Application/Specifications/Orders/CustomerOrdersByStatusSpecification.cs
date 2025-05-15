using System.Linq.Expressions;
using CQRSSolution.Domain.Entities;

namespace CQRSSolution.Application.Specifications.Orders;

/// <summary>
///     Specification to retrieve orders for a specific customer by their email,
///     filtered by order status, ordered by order date descending, and paginated.
/// </summary>
public sealed class CustomerOrdersByStatusSpecification : BaseSpecification<Order>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomerOrdersByStatusSpecification" /> class.
    /// </summary>
    /// <param name="customerEmail">
    ///     The email of the customer whose orders are to be retrieved. If null or empty, this filter
    ///     is not applied.
    /// </param>
    /// <param name="status">The status to filter orders by. If null or empty, this filter is not applied.</param>
    /// <param name="pageNumber">The page number for pagination (1-indexed).</param>
    /// <param name="pageSize">The number of items per page for pagination.</param>
    public CustomerOrdersByStatusSpecification(
        string? customerEmail,
        string? status,
        int pageNumber,
        int pageSize)
    {
        // Build criteria dynamically
        Expression<Func<Order, bool>>? criteria = null;

        if (!string.IsNullOrWhiteSpace(customerEmail))
        {
            Expression<Func<Order, bool>> customerEmailCriteria =
                o => o.Customer != null && o.Customer.Email == customerEmail;
            criteria = customerEmailCriteria;
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            Expression<Func<Order, bool>> statusCriteria = o => o.Status == status;
            if (criteria != null)
            {
                var invokedExpr = Expression.Invoke(statusCriteria, criteria.Parameters[0]);
                criteria = Expression.Lambda<Func<Order, bool>>(Expression.AndAlso(criteria.Body, invokedExpr),
                    criteria.Parameters);
            }
            else
            {
                criteria = statusCriteria;
            }
        }

        if (criteria != null) Criteria = criteria;

        // Apply ordering
        ApplyOrderByDescending(o => o.OrderDate);

        // Apply pagination
        // Ensure pageNumber is at least 1
        var validPageNumber = Math.Max(1, pageNumber);
        var skip = (validPageNumber - 1) * pageSize;
        ApplyPaging(skip, pageSize);

        // Include related entities necessary for the OrderSummaryDto
        AddInclude(o => o.Customer); // Needed for CustomerName
        AddInclude(o => o.OrderItems); // Needed for ItemCount
    }
}