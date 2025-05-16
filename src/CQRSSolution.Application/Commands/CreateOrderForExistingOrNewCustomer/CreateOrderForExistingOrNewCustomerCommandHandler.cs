using CQRSSolution.Application.Factories;
using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.DomainEvents;
using CQRSSolution.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CQRSSolution.Application.Commands.CreateOrderForExistingOrNewCustomer;

/// <summary>
/// Handles creating a new order for either an existing or new customer,
/// ensuring all operations are performed within a single transaction.
/// </summary>
public class CreateOrderForExistingOrNewCustomerCommandHandler 
    : IRequestHandler<CreateOrderForExistingOrNewCustomerCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderFactory _orderFactory;
    private readonly IOutboxMessageFactory _outboxMessageFactory;
    private readonly ILogger<CreateOrderForExistingOrNewCustomerCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrderForExistingOrNewCustomerCommandHandler"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for managing transactions and repository access.</param>
    /// <param name="orderFactory">The factory for creating order instances.</param>
    /// <param name="outboxMessageFactory">The factory for creating outbox message instances.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the dependencies are null.</exception>
    public CreateOrderForExistingOrNewCustomerCommandHandler(
        IUnitOfWork unitOfWork,
        IOrderFactory orderFactory,
        IOutboxMessageFactory outboxMessageFactory,
        ILogger<CreateOrderForExistingOrNewCustomerCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderFactory = orderFactory ?? throw new ArgumentNullException(nameof(orderFactory));
        _outboxMessageFactory = outboxMessageFactory ?? throw new ArgumentNullException(nameof(outboxMessageFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handles the command to create an order for an existing or new customer.
    /// </summary>
    /// <param name="command">The command containing customer and order details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly created order.</returns>
    /// <exception cref="AggregateException">Thrown when both the transaction and rollback fail.</exception>
    public async Task<Guid> Handle(
        CreateOrderForExistingOrNewCustomerCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing order creation for customer email: {CustomerEmail}", command.CustomerEmail);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var orderId = await ProcessOrderCreationAsync(command, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return orderId;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(ex, command.CustomerEmail, cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Processes the creation of an order within a transaction, including customer lookup or creation, 
    /// order creation, and domain event publication via the outbox pattern.
    /// </summary>
    /// <param name="command">The command containing the order and customer details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly created order.</returns>
    private async Task<Guid> ProcessOrderCreationAsync(
        CreateOrderForExistingOrNewCustomerCommand command, 
        CancellationToken cancellationToken)
    {
        var customer = await GetOrCreateCustomerAsync(
            command.CustomerEmail, 
            command.CustomerName, 
            cancellationToken);

        var order = _orderFactory.CreateNewOrder(customer, command.Items);
        
        await _unitOfWork.Orders.AddAsync(order, cancellationToken);

        await CreateOrderCreatedEventAsync(order, customer, cancellationToken);
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} created successfully for customer {CustomerId} ({CustomerEmail})",
            order.OrderId,
            customer.CustomerId,
            customer.Email);

        return order.OrderId;
    }

    /// <summary>
    /// Retrieves an existing customer by email or creates a new one if not found.
    /// </summary>
    /// <param name="email">The email address of the customer to find or create.</param>
    /// <param name="name">The name to use when creating a new customer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The existing or newly created customer entity.</returns>
    private async Task<Customer> GetOrCreateCustomerAsync(
        string email, 
        string name,
        CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByEmailAsync(email, cancellationToken);

        if (customer != null)
        {
            _logger.LogInformation("Using existing customer: {CustomerId} ({CustomerEmail})", customer.CustomerId, customer.Email);
            
            return customer;
        }

        _logger.LogInformation("Creating new customer with email: {CustomerEmail}", email);
        
        var newCustomer = Customer.Create(name, email);
        
        await _unitOfWork.Customers.AddAsync(newCustomer, cancellationToken);
        
        return newCustomer;
    }

    /// <summary>
    /// Creates an OrderCreatedDomainEvent and stores it as an outbox message to ensure eventual consistency.
    /// </summary>
    /// <param name="order">The newly created order.</param>
    /// <param name="customer">The customer associated with the order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task CreateOrderCreatedEventAsync(
        Order order, 
        Customer customer,
        CancellationToken cancellationToken)
    {
        var orderCreatedEvent = new OrderCreatedDomainEvent(
            order.OrderId,
            customer.Name,
            order.OrderDate);

        var outboxMessage = _outboxMessageFactory.CreateFromDomainEvent(orderCreatedEvent);
        
        await _unitOfWork.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
    }

    /// <summary>
    /// Handles exceptions that occur during order creation by rolling back the transaction and logging appropriate messages.
    /// </summary>
    /// <param name="exception">The exception that was thrown during order creation.</param>
    /// <param name="customerEmail">The email of the customer for which the order was being created.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="AggregateException">Thrown when both the transaction and rollback fail.</exception>
    private async Task HandleExceptionAsync(
        Exception exception, 
        string customerEmail,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error creating order for customer {CustomerEmail}. Rolling back transaction.", customerEmail);

        try
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            
            _logger.LogInformation("Transaction successfully rolled back for {CustomerEmail}", customerEmail);
        }
        catch (Exception rollbackEx)
        {
            _logger.LogError(rollbackEx, "Failed to roll back transaction for {CustomerEmail}", customerEmail);
            
            throw new AggregateException("Transaction failed and rollback also failed", exception, rollbackEx);
        }
    }
}