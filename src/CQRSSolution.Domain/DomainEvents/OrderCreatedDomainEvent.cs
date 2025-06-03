using System;

namespace CQRSSolution.Domain.DomainEvents;

/// <summary>
///     Represents an event that is raised when an order is successfully created.
/// </summary>
public class OrderCreatedDomainEvent
{
    /// <summary>
    ///     Gets the unique identifier of the order that was created.
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    ///     Gets the name of the customer who placed the order.
    /// </summary>
    public string CustomerName { get; }

    /// <summary>
    ///     Gets the total amount of the created order.
    /// </summary>
    public decimal TotalAmount { get; }

    /// <summary>
    ///     Gets the date and time when the order was created.
    /// </summary>
    public DateTime OrderDate { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderCreatedDomainEvent" /> class.
    /// </summary>
    /// <param name="orderId">The ID of the created order.</param>
    /// <param name="customerName">The name of the customer.</param>
    /// <param name="totalAmount">The total amount of the order.</param>
    /// <param name="orderDate">The date the order was placed.</param>
    public OrderCreatedDomainEvent(Guid orderId, string customerName, decimal totalAmount, DateTime orderDate)
    {
        OrderId = orderId;
        CustomerName = customerName;
        TotalAmount = totalAmount;
        OrderDate = orderDate;
    }
}