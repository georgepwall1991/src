namespace CQRSSolution.Application.DTOs;

/// <summary>
///     Data Transfer Object for an order item when creating an order.
/// </summary>
public class OrderItemDto
{
    /// <summary>
    ///     Gets or sets the name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the quantity of the product.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    ///     Gets or sets the price per unit of the product.
    /// </summary>
    public decimal UnitPrice { get; set; }
}