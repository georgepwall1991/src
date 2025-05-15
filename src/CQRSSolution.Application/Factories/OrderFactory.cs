using CQRSSolution.Application.DTOs;
using CQRSSolution.Domain.Entities;

namespace CQRSSolution.Application.Factories;

/// <summary>
///     Factory responsible for creating <see cref="Order" /> instances.
/// </summary>
public class OrderFactory : IOrderFactory
{
    private const string DefaultOrderStatus = "Pending";

    /// <summary>
    ///     Creates a new <see cref="Order" /> instance with its associated items.
    /// </summary>
    /// <param name="customer">The customer placing the order.</param>
    /// <param name="orderItemsDto">A list of DTOs representing the items in the order.</param>
    /// <returns>A new <see cref="Order" /> instance.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="customer" /> or <paramref name="orderItemsDto" /> is
    ///     null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="orderItemsDto" /> is empty.</exception>
    public Order CreateNewOrder(Customer customer, List<OrderItemDto> orderItemsDto)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(orderItemsDto);
        if (orderItemsDto.Count == 0)
            throw new ArgumentException("Order must contain at least one item.", nameof(orderItemsDto));

        var order = new Order(
            customer.CustomerId,
            customer.Name,
            DateTime.UtcNow,
            DefaultOrderStatus
        );

        foreach (var itemDto in orderItemsDto)
            order.AddOrderItem(itemDto.ProductName, itemDto.Quantity, itemDto.UnitPrice);

        return order;
    }
}

/// <summary>
///     Interface for the <see cref="OrderFactory" />.
/// </summary>
public interface IOrderFactory
{
    /// <summary>
    ///     Creates a new <see cref="Order" /> instance with its associated items.
    /// </summary>
    /// <param name="customer">The customer placing the order.</param>
    /// <param name="orderItemsDto">A list of DTOs representing the items in the order.</param>
    /// <returns>A new <see cref="Order" /> instance.</returns>
    Order CreateNewOrder(Customer customer, List<OrderItemDto> orderItemsDto);
}