namespace CQRSSolution.Application.DTOs;

/// <summary>
///     Data Transfer Object for an order item when retrieving order details.
/// </summary>
public class OrderItemDetailsDto
{
    /// <summary>
    ///     Gets or sets the unique identifier for the order item.
    /// </summary>
    public Guid OrderItemId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the quantity of this product in the order.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    ///     Gets or sets the price per unit of the product.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    ///     Gets or sets the total price for this line item (Quantity * UnitPrice).
    /// </summary>
    public decimal LineTotal => Quantity * UnitPrice;
}