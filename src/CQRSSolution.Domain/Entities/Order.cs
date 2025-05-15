namespace CQRSSolution.Domain.Entities;

/// <summary>
///     Represents an order in the system.
/// </summary>
public class Order
{
    private readonly List<OrderItem> _orderItems = new(); // Backing field

    // Private constructor for EF Core
    private Order()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Order" /> class.
    /// </summary>
    /// <param name="customerId">The ID of the customer placing the order.</param>
    /// <param name="customerName">The name of the customer.</param>
    /// <param name="orderDate">The date the order was placed.</param>
    /// <param name="status">The initial status of the order.</param>
    public Order(Guid customerId, string customerName, DateTime orderDate, string status)
    {
        OrderId = Guid.NewGuid();
        CustomerId = customerId;
        CustomerName = customerName;
        OrderDate = orderDate;
        Status = status;
        TotalAmount = 0; // Initialized to 0, updated by AddOrderItem
    }

    /// <summary>
    ///     Gets the unique identifier for the order.
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    ///     Gets the ID of the customer who placed the order.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    ///     Gets or sets the customer who placed the order.
    /// </summary>
    public virtual Customer? Customer { get; set; } // Keep setter for EF Core relationship fixup

    /// <summary>
    ///     Gets the name of the customer who placed the order.
    /// </summary>
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the date and time when the order was placed.
    /// </summary>
    public DateTime OrderDate { get; private set; }

    /// <summary>
    ///     Gets the total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>
    ///     Gets the status of the order (e.g., Pending, Confirmed, Shipped).
    /// </summary>
    public string Status { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the collection of items included in this order.
    /// </summary>
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    /// <summary>
    ///     Adds an item to the order and updates the total amount.
    /// </summary>
    /// <param name="productName">The name of the product.</param>
    /// <param name="quantity">The quantity of the product.</param>
    /// <param name="unitPrice">The unit price of the product.</param>
    /// <exception cref="ArgumentException">Thrown if productName is empty, quantity is zero or less, or unitPrice is negative.</exception>
    public void AddOrderItem(string productName, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty.", nameof(productName));
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        var orderItem = new OrderItem(OrderId, productName, quantity, unitPrice);
        _orderItems.Add(orderItem);
        TotalAmount += orderItem.Quantity * orderItem.UnitPrice;
    }

    /// <summary>
    ///     Updates the status of the order.
    /// </summary>
    /// <param name="newStatus">The new status.</param>
    /// <exception cref="ArgumentException">Thrown if newStatus is null or whitespace.</exception>
    public void UpdateStatus(string newStatus)
    {
        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("Status cannot be empty.", nameof(newStatus));
        Status = newStatus;
    }
}