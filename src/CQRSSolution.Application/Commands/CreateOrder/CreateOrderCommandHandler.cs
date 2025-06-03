using MediatR;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.Entities;
using CQRSSolution.Domain.DomainEvents;
using Microsoft.Extensions.Logging;
using System.Linq; // Required for .Any() and .Sum()

namespace CQRSSolution.Application.Commands.CreateOrder;

/// <summary>
///     Handles the <see cref="CreateOrderCommand" /> to create a new order for a customer.
///     This handler orchestrates finding or creating a customer, creating the order and its items,
///     and ensuring an <see cref="OrderCreatedDomainEvent" /> is stored as an <see cref="OutboxMessage" />.
///     All database operations are performed within a single atomic transaction managed by <see cref="IUnitOfWork" />.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateOrderCommandHandler" /> class.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    /// <param name="logger">The logger.</param>
    public CreateOrderCommandHandler(IApplicationDbContext dbContext, ILogger<CreateOrderCommandHandler> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false // Compact JSON for storage
        };
    }

    /// <summary>
    ///     Handles the command to create an order.
    /// </summary>
    /// <param name="command">The <see cref="CreateOrderCommand" /> containing order details.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The ID of the newly created order.</returns>
    /// <exception cref="InvalidOperationException">Thrown if critical operations fail, like event type resolution.</exception>
    /// <exception cref="Exception">Rethrows exceptions from database operations after attempting a rollback.</exception>
    public async Task<Guid> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        if (command == null) throw new ArgumentNullException(nameof(command));
        if (string.IsNullOrWhiteSpace(command.CustomerName))
            throw new ArgumentException("Customer name is required.", nameof(command.CustomerName));
        if (command.Items == null || !command.Items.Any())
            throw new ArgumentException("Order must contain at least one item.", nameof(command.Items));

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var order = new Order
            {
                CustomerName = command.CustomerName,
                // OrderDate, Status, and Id are set in Order constructor
            };

            foreach (var itemDto in command.Items)
            {
                // OrderItem constructor performs its own validation for ProductName, quantity, and unit price.
                order.AddOrderItem(itemDto.ProductName, itemDto.Quantity, itemDto.UnitPrice);
            }
            // TotalAmount is calculated by AddOrderItem and RecalculateTotalAmount

            _dbContext.Orders.Add(order);
            // OrderItems are implicitly added by EF Core as they are part of the Order.OrderItems collection
            // and Order is being added.

            var orderCreatedEvent = new OrderCreatedDomainEvent(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.OrderDate
            );

            var outboxMessage = new OutboxMessage
            {
                // Id and OccurredOnUtc are set in OutboxMessage constructor
                Type = orderCreatedEvent.GetType().FullName ?? nameof(OrderCreatedDomainEvent),
                Payload = JsonSerializer.Serialize(orderCreatedEvent, _jsonSerializerOptions)
            };
            _dbContext.OutboxMessages.Add(outboxMessage);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerName}.", order.Id, order.CustomerName);

            return order.Id;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error creating order for customer {CustomerName}. Details: {ErrorMessage}", command.CustomerName, ex.Message);
            throw; // Re-throw to allow higher layers (e.g., API) to handle it.
        }
    }
}