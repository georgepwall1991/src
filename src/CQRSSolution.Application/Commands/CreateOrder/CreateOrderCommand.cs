using CQRSSolution.Application.DTOs;
using MediatR;

namespace CQRSSolution.Application.Commands.CreateOrder;

/// <summary>
///     Command to create a new order. Requires customer name and email to find or create a customer record.
/// </summary>
public class CreateOrderCommand : IRequest<Guid>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateOrderCommand" /> class.
    /// </summary>
    /// <param name="customerName">The customer name.</param>
    /// <param name="customerEmail">The customer email.</param>
    /// <param name="items">The list of order items.</param>
    public CreateOrderCommand(string customerName, string customerEmail, List<OrderItemDto> items)
    {
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        Items = items;
    }

    /// <summary>
    ///     Gets or sets the customer's name.
    /// </summary>
    public string CustomerName { get; set; }

    /// <summary>
    ///     Gets or sets the customer's email address. Used to find an existing customer or create a new one.
    /// </summary>
    public string CustomerEmail { get; set; }

    /// <summary>
    ///     Gets or sets the list of items for the order.
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = new();
}