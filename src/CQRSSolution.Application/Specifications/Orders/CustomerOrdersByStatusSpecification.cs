using System.Linq.Expressions;
using CQRSSolution.Domain.Entities;

namespace CQRSSolution.Application.Specifications.Orders;

/// <summary>
/// Specification to retrieve orders for a specific customer by their email,
/// filtered by order status, ordered by order date descending, and paginated.
/// </summary>
public sealed class CustomerOrdersByStatusSpecification : BaseSpecification<Order>
{
    private const int MinPageNumber = 1;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerOrdersByStatusSpecification"/> class.
    /// </summary>
    /// <param name="customerEmail">The email of the customer whose orders are to be retrieved.</param>
    /// <param name="status">The status to filter orders by.</param>
    /// <param name="pageNumber">The page number for pagination (1-indexed).</param>
    /// <param name="pageSize">The number of items per page for pagination.</param>
    public CustomerOrdersByStatusSpecification(
        string? customerEmail,
        string? status,
        int pageNumber,
        int pageSize)
    {
        // Apply filter criteria
        ApplyFilterCriteria(customerEmail, status);
        
        // Configure sorting
        ApplyOrderByDescending(o => o.OrderDate);
        
        // Configure pagination
        ApplyPagination(pageNumber, pageSize);
        
        // Include related entities
        ConfigureIncludes();
    }
    
    private void ApplyFilterCriteria(string? customerEmail, string? status)
    {
        var customerCriteria = BuildCustomerEmailCriteria(customerEmail);
        var statusCriteria = BuildStatusCriteria(status);
        
        if (customerCriteria != null && statusCriteria != null)
        {
            Criteria = CombineCriteria(customerCriteria, statusCriteria);
        }
        else
        {
            Criteria = customerCriteria ?? statusCriteria;
        }
    }
    
    private static Expression<Func<Order, bool>>? BuildCustomerEmailCriteria(string? customerEmail)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
            return null;
            
        return order => order.Customer != null && order.Customer.Email == customerEmail;
    }
    
    private static Expression<Func<Order, bool>>? BuildStatusCriteria(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;
            
        return order => order.Status == status;
    }
    
    private static Expression<Func<Order, bool>> CombineCriteria(
        Expression<Func<Order, bool>> firstCriteria, 
        Expression<Func<Order, bool>> secondCriteria)
    {
        var parameter = firstCriteria.Parameters[0];
        var invokedSecond = Expression.Invoke(secondCriteria, parameter);
        
        var combinedBody = Expression.AndAlso(firstCriteria.Body, invokedSecond);
        return Expression.Lambda<Func<Order, bool>>(combinedBody, parameter);
    }
    
    private void ApplyPagination(int pageNumber, int pageSize)
    {
        var validPageNumber = Math.Max(MinPageNumber, pageNumber);
        var skip = (validPageNumber - 1) * pageSize;
        ApplyPaging(skip, pageSize);
    }
    
    private void ConfigureIncludes()
    {
        AddInclude(o => o.Customer);    // Required for customer information
        AddInclude(o => o.OrderItems);  // Required for order details
    }
}