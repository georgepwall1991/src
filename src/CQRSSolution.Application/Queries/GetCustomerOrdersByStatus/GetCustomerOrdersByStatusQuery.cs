using CQRSSolution.Application.DTOs;
using MediatR;

namespace CQRSSolution.Application.Queries.GetCustomerOrdersByStatus;

/// <summary>
///     Query to retrieve a paginated list of orders for a specific customer by email and status.
/// </summary>
public class GetCustomerOrdersByStatusQuery : IRequest<PagedResultDto<OrderSummaryDto>>
{
    /// <summary>
    ///     Gets or sets the email of the customer. If null or empty, orders are not filtered by customer email.
    /// </summary>
    public string? CustomerEmail { get; set; }

    /// <summary>
    ///     Gets or sets the status of the orders to retrieve. If null or empty, orders are not filtered by status.
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    ///     Gets or sets the page number (1-indexed).
    ///     Defaults to 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the number of items per page.
    ///     Defaults to 10.
    /// </summary>
    public int PageSize { get; set; } = 10;
}