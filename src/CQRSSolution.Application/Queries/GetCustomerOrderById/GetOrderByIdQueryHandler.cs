using CQRSSolution.Application.DTOs;
using CQRSSolution.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CQRSSolution.Application.Queries.GetCustomerOrderById;

/// <summary>
///     Handles the <see cref="GetOrderByIdQuery" /> to retrieve a specific order by its unique identifier.
///     In a CQRS (Command Query Responsibility Segregation) architecture:
///     - Queries are responsible for reading and returning data, without altering the system's state.
///     They typically return Data Transfer Objects (DTOs) tailored for specific read needs.
///     - Commands, in contrast, are responsible for changing the system's state (e.g., creating, updating, deleting
///     entities)
///     and usually do not return data beyond a success/failure indicator or an entity identifier.
///     This handler exemplifies the Query side, fetching an <see cref="OrderDto" /> using the injected
///     <see cref="IOrderRepository" />.
/// </summary>
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly ILogger<GetOrderByIdQueryHandler> _logger;
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, ILogger<GetOrderByIdQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching order with ID: {OrderId}", request.OrderId);

        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);

        if (order == null)
        {
            _logger.LogWarning("Order with ID: {OrderId} not found.", request.OrderId);
            return null;
        }

        // Manual mapping from Entity to DTO
        // Could use AutoMapper or Mapster 
        var orderDto = new OrderDto
        {
            OrderId = order.OrderId,
            CustomerName = order.CustomerName,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Items = order.OrderItems.Select(oi => new OrderItemDetailsDto
            {
                OrderItemId = oi.OrderItemId,
                ProductName = oi.ProductName,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList()
        };

        _logger.LogInformation("Successfully fetched order with ID: {OrderId}", request.OrderId);
        return orderDto;
    }
}