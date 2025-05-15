using CQRSSolution.Application.DTOs;
using MediatR;

namespace CQRSSolution.Application.Commands.CreateOrderForExistingOrNewCustomer;

/// <summary>
///     Command to create an order for a customer. If the customer does not exist (based on email),
///     a new customer record is created.
/// </summary>
public class CreateOrderForExistingOrNewCustomerCommand : IRequest<Guid> // Returns the OrderId
{
    /// <summary>
    ///     Gets or sets the customer's name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the customer's email address.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the list of items in the order.
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = new();
}