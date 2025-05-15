namespace CQRSSolution.Application.DTOs;

/// <summary>
///     Data Transfer Object for retrieving order details.
/// </summary>
public class OrderDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the order.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the customer who placed the order.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time when the order was placed.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    ///     Gets or sets the total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    ///     Gets or sets the status of the order.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the list of items in the order.
    /// </summary>
    public List<OrderItemDetailsDto> Items { get; set; } = new();
}