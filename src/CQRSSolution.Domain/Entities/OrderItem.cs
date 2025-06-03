using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CQRSSolution.Domain.Entities;

/// <summary>
///     Represents an item within an order.
/// </summary>
public class OrderItem
{
    /// <summary>
    ///     Gets or sets the unique identifier for the order item.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the foreign key referencing the Order.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    ///     Gets or sets the navigation property to the Order.
    ///     This is virtual to enable lazy loading by EF Core.
    /// </summary>
    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    /// <summary>
    ///     Gets or sets the name of the product.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; }

    /// <summary>
    ///     Gets or sets the quantity of the product.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    ///     Gets or sets the price per unit of the product.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    ///     Private parameterless constructor for EF Core and deserialization.
    ///     Initializes Product Name to empty string to avoid null warnings.
    /// </summary>
    private OrderItem()
    {
        ProductName = string.Empty;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderItem" /> class.
    /// </summary>
    /// <param name="orderId">The ID of the parent order.</param>
    /// <param name="productName">The name of the product.</param>
    /// <param name="quantity">The quantity of the product.</param>
    /// <param name="unitPrice">The unit price of the product.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if quantity is not positive or unit price is negative.</exception>
    /// <exception cref="ArgumentNullException">Thrown if productName is null or whitespace.</exception>
    public OrderItem(Guid orderId, string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentNullException(nameof(productName));
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");

        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}