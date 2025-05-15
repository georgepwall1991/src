namespace CQRSSolution.Application.DTOs;

/// <summary>
///     Data Transfer Object for creating a new order.
/// </summary>
public class CreateOrderRequestDto
{
    /// <summary>
    ///     Gets or sets the name of the customer.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;


    /// <summary>
    ///     Gets or sets the email address of the customer.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the list of items in the order.
    /// </summary>
    public List<OrderItemDto> Items { get; set; } = new();
}