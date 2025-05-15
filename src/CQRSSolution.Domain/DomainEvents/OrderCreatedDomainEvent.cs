namespace CQRSSolution.Domain.DomainEvents;

/// <summary>
///     Represents an event that is raised when an order is created.
/// </summary>
public class OrderCreatedDomainEvent
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderCreatedDomainEvent" /> class.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="customerName">The customer name.</param>
    /// <param name="orderDate">The order date.</param>
    public OrderCreatedDomainEvent(Guid orderId, string customerName, DateTime orderDate)
    {
        OrderId = orderId;
        CustomerName = customerName;
        OrderDate = orderDate;
    }

    /// <summary>
    ///     Gets the unique identifier of the created order.
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    ///     Gets the name of the customer who placed the order.
    /// </summary>
    public string CustomerName { get; }

    /// <summary>
    ///     Gets the date and time when the order was created.
    /// </summary>
    public DateTime OrderDate { get; }
}