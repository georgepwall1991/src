using CQRSSolution.Application.DTOs;
using MediatR;

namespace CQRSSolution.Application.Queries.GetCustomerOrderById;

/// <summary>
///     Query to retrieve an order by its unique identifier.
/// </summary>
public class GetOrderByIdQuery : IRequest<OrderDto?> // OrderDto can be null if not found
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GetOrderByIdQuery" /> class.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    public GetOrderByIdQuery(Guid orderId)
    {
        OrderId = orderId;
    }

    /// <summary>
    ///     Gets the unique identifier of the order to retrieve.
    /// </summary>
    public Guid OrderId { get; }
}