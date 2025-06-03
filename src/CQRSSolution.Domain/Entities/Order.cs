using CQRSSolution.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CQRSSolution.Domain.Entities;

/// <summary>
///     Represents an order in the system.
/// </summary>
public class Order
{
    /// <summary>
    ///     Gets or sets the unique identifier for the order.
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the name of the customer who placed the order.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time when the order was placed.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    ///     Gets or sets the total amount for the order.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>
    ///     Gets or sets the current status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    ///     Gets or sets the collection of items included in this order.
    /// </summary>
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="Order" /> class.
    /// </summary>
    /// <param name="customerId">The ID of the customer placing the order.</param>
    /// <param name="customerName">The name of the customer.</param>
    /// <param name="orderDate">The date the order was placed.</param>
    /// <param name="status">The initial status of the order.</param>
    public Order(Guid customerId, string customerName, DateTime orderDate, string status)
    {
        Id = Guid.NewGuid();
        CustomerId = customerId;
        CustomerName = customerName;
        OrderDate = orderDate;
        Status = status;
        TotalAmount = 0; // Initialized to 0, updated by AddOrderItem
    }

    /// <summary>
    ///     Gets the ID of the customer who placed the order.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    ///     Gets or sets the customer who placed the order.
    /// </summary>
    public virtual Customer? Customer { get; set; } // Keep setter for EF Core relationship fixup

    /// <summary>
    ///     Initializes a new instance of the <see cref="Order"/> class.
    ///     Sets default values for new orders.
    /// </summary>
    public Order()
    {
        Id = Guid.NewGuid();
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending; // Use Enum directly
        OrderItems = new List<OrderItem>();
        CustomerName = string.Empty; // Initialize to prevent null reference
    }

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

        var orderItem = new OrderItem(Id, productName, quantity, unitPrice);
        OrderItems.Add(orderItem);
        RecalculateTotalAmount();
    }

    /// <summary>
    ///     Recalculates the total amount of the order based on its items.
    /// </summary>
    public void RecalculateTotalAmount()
    {
        TotalAmount = OrderItems.Sum(item => item.Quantity * item.UnitPrice);
    }

    /// <summary>
    ///     Updates the status of the order.
    /// </summary>
    /// <param name="newStatus">The new status.</param>
    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
    }
}