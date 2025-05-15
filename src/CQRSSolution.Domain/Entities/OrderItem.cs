namespace CQRSSolution.Domain.Entities;

/// <summary>
///     Represents an item within an order.
/// </summary>
public class OrderItem
{
    // Private constructor for EF Core
    private OrderItem()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderItem" /> class.
    /// </summary>
    /// <param name="orderId">The ID of the order this item belongs to.</param>
    /// <param name="productName">The name of the product.</param>
    /// <param name="quantity">The quantity of the product.</param>
    /// <param name="unitPrice">The unit price of the product.</param>
    /// <exception cref="ArgumentException">Thrown if productName is empty, quantity is zero or less, or unitPrice is negative.</exception>
    internal OrderItem(Guid orderId, string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty.", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        OrderItemId = Guid.NewGuid();
        OrderId = orderId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>
    ///     Gets the unique identifier for the order item.
    /// </summary>
    public Guid OrderItemId { get; private set; }

    /// <summary>
    ///     Gets the foreign key referencing the Order this item belongs to.
    /// </summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    ///     Gets or sets the navigation property to the Order this item belongs to.
    /// </summary>
    public virtual Order? Order { get; set; } // Setter for EF Core relationship fixup

    /// <summary>
    ///     Gets the name of the product.
    /// </summary>
    public string ProductName { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the quantity of this product in the order.
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    ///     Gets the price per unit of the product.
    /// </summary>
    public decimal UnitPrice { get; private set; }
}