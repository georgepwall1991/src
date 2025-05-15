using CQRSSolution.Application.Factories;
using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.DomainEvents;
using CQRSSolution.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CQRSSolution.Application.Commands.CreateOrderForExistingOrNewCustomer;

/// <summary>
///     Handles the <see cref="CreateOrderForExistingOrNewCustomerCommand" /> to create a new order.
///     This handler orchestrates the process of either finding an existing customer by email or creating a new one,
///     then creating the order with its items, and finally ensuring an <see cref="OrderCreatedDomainEvent" />
///     is stored as an <see cref="OutboxMessage" />. All database operations (customer creation/update,
///     order creation, outbox message creation) are performed within a single atomic transaction
///     managed by the injected <see cref="IUnitOfWork" />.
/// </summary>
/// <remarks>
///     Workflow:
///     1. Begins a database transaction using <see cref="IUnitOfWork" />.
///     2. Attempts to find an existing customer using <see cref="ICustomerRepository.GetByEmailAsync" /> via the Unit of
///     Work.
///     3. If the customer does not exist, a new <see cref="Customer" /> is created using
///     <see cref="Customer.Create(string, string)" />
///     and added to the repository via <see cref="IUnitOfWork.Customers" />.
///     4. A new <see cref="Order" /> is created using the <see cref="IOrderFactory" />, linking it to the customer and
///     populating its items from the command. The order is then added via <see cref="IUnitOfWork.Orders" />.
///     5. An <see cref="OrderCreatedDomainEvent" /> is instantiated.
///     6. An <see cref="OutboxMessage" /> is created from this domain event using the <see cref="IOutboxMessageFactory" />
///     and added via <see cref="IUnitOfWork.OutboxMessages" />.
///     7. All pending changes across repositories are saved to the database by calling
///     <see cref="IUnitOfWork.CompleteAsync" />.
///     8. If all operations are successful, the transaction is committed using
///     <see cref="IUnitOfWork.CommitTransactionAsync" />.
///     9. If any exception occurs during this process, the transaction is rolled back using
///     <see cref="IUnitOfWork.RollbackTransactionAsync" />,
///     ensuring data consistency, and the exception is re-thrown.
///     The handler returns the ID of the newly created order upon success.
/// </remarks>
public class
    CreateOrderForExistingOrNewCustomerCommandHandler : IRequestHandler<CreateOrderForExistingOrNewCustomerCommand,
    Guid>
{
    private const string OrderStatusPending = "Pending";
    private readonly ILogger<CreateOrderForExistingOrNewCustomerCommandHandler> _logger;
    private readonly IOrderFactory _orderFactory;
    private readonly IOutboxMessageFactory _outboxMessageFactory;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateOrderForExistingOrNewCustomerCommandHandler" /> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for managing transactions and repository access.</param>
    /// <param name="orderFactory">The factory for creating order instances.</param>
    /// <param name="outboxMessageFactory">The factory for creating outbox message instances.</param>
    /// <param name="logger">The logger for logging information and errors.</param>
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
    ///     Handles the command to create an order for an existing or new customer.
    /// </summary>
    /// <param name="command">The command containing customer and order details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the newly created order.</returns>
    public async Task<Guid> Handle(CreateOrderForExistingOrNewCustomerCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing CreateOrderForExistingOrNewCustomerCommand for customer email: {CustomerEmail}",
            command.CustomerEmail);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var customer = await _unitOfWork.Customers.GetByEmailAsync(command.CustomerEmail, cancellationToken);

            if (customer == null)
            {
                _logger.LogInformation("Customer with email {CustomerEmail} not found. Creating new customer.",
                    command.CustomerEmail);
                customer = Customer.Create(command.CustomerName, command.CustomerEmail);
                await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Existing customer {CustomerId} (Email: {CustomerEmail}) found.",
                    customer.CustomerId, customer.Email);
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
                "Transaction committed via UnitOfWork. Order {OrderId} created for customer {CustomerId} (Email: {CustomerEmail}).",
                order.OrderId,
                customer.CustomerId,
                customer.Email);

            return order.OrderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing CreateOrderForExistingOrNewCustomerCommand for customer email {CustomerEmail}. Rolling back transaction.",
                command.CustomerEmail);
            try
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                _logger.LogInformation("Transaction rolled back via UnitOfWork for customer email {CustomerEmail}.",
                    command.CustomerEmail);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx,
                    "An error occurred during transaction rollback via UnitOfWork for customer {CustomerEmail}.",
                    command.CustomerEmail);
                throw new AggregateException(ex, rollbackEx);
            }

            throw;
        }
    }
}