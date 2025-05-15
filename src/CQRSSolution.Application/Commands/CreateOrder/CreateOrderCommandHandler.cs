using CQRSSolution.Application.Factories;
using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.DomainEvents;
using CQRSSolution.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CQRSSolution.Application.Commands.CreateOrder;

/// <summary>
///     Handles the <see cref="CreateOrderCommand" /> to create a new order for a customer.
///     This handler orchestrates finding or creating a customer, creating the order and its items,
///     and ensuring an <see cref="OrderCreatedDomainEvent" /> is stored as an <see cref="OutboxMessage" />.
///     All database operations are performed within a single atomic transaction managed by <see cref="IUnitOfWork" />.
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IOrderFactory _orderFactory;
    private readonly IOutboxMessageFactory _outboxMessageFactory;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateOrderCommandHandler" /> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for managing database operations and transactions.</param>
    /// <param name="orderFactory">The factory for creating <see cref="Order" /> instances.</param>
    /// <param name="outboxMessageFactory">The factory for creating <see cref="OutboxMessage" /> instances.</param>
    /// <param name="logger">The logger for this handler.</param>
    public CreateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IOrderFactory orderFactory,
        IOutboxMessageFactory outboxMessageFactory,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderFactory = orderFactory ?? throw new ArgumentNullException(nameof(orderFactory));
        _outboxMessageFactory = outboxMessageFactory ?? throw new ArgumentNullException(nameof(outboxMessageFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        _logger.LogInformation("Processing CreateOrderCommand for customer email: {CustomerEmail}", command.CustomerEmail);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await _unitOfWork.Customers.GetByEmailAsync(command.CustomerEmail, cancellationToken);

            if (customer == null)
            {
                _logger.LogInformation("Customer with email {CustomerEmail} not found for CreateOrderCommand. Creating new customer.", command.CustomerEmail);
                
                customer = Customer.Create(command.CustomerName, command.CustomerEmail);
                
                await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
            }

            else
            {
                _logger.LogInformation("Existing customer {CustomerId} (Email: {CustomerEmail}) found for CreateOrderCommand.", customer.CustomerId, customer.Email);
            }

            var order = _orderFactory.CreateNewOrder(customer, command.Items);
            
            await _unitOfWork.Orders.AddAsync(order, cancellationToken);

            var orderCreatedEvent = new OrderCreatedDomainEvent(
                order.OrderId,
                customer.Name,
                order.OrderDate);

            var outboxMessage = _outboxMessageFactory.CreateFromDomainEvent(orderCreatedEvent);
            
            await _unitOfWork.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

            await _unitOfWork.CompleteAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Transaction committed. Order {OrderId} created for customer {CustomerId} (Email: {CustomerEmail}).",
                order.OrderId,
                customer.CustomerId,
                command.CustomerEmail);

            return order.OrderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CreateOrderCommand for customer email {CustomerEmail}. Rolling back transaction.", command.CustomerEmail);
            
            try
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                
                _logger.LogInformation("Transaction rolled back for CreateOrderCommand (CustomerEmail: {CustomerEmail}).", command.CustomerEmail);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx,"An error occurred during transaction rollback for CreateOrderCommand (CustomerEmail: {CustomerEmail}).", command.CustomerEmail);
                throw new AggregateException(ex, rollbackEx);
            }

            throw;
        }
    }
}