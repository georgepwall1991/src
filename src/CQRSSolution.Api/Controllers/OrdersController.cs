
using CQRSSolution.Application.Commands.CreateOrder;
using CQRSSolution.Application.DTOs;
using CQRSSolution.Application.Queries.GetCustomerOrderById;
using CQRSSolution.Application.Queries.GetCustomerOrdersByStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CQRSSolution.Api.Controllers;

/// <summary>
/// API controller for managing orders in the system.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IMediator _mediator;
    
    private const int MaxPageSize = 100;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance for sending commands and queries.</param>
    /// <param name="logger">The logger for recording controller activity.</param>
    /// <exception cref="ArgumentNullException">Thrown if any dependency is null.</exception>
    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new order in the system.
    /// </summary>
    /// <param name="createOrderDto">The order creation request data.</param>
    /// <returns>The ID of the newly created order.</returns>
    /// <response code="201">Returns the newly created order's ID.</response>
    /// <response code="400">If the request is invalid (e.g., validation errors).</response>
    /// <response code="500">If an unexpected error occurs on the server.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto createOrderDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("CreateOrder called with invalid model state.");
            return BadRequest(ModelState);
        }

        try
        {
            var orderId = await CreateOrderAsync(createOrderDto);
            return CreatedAtAction(nameof(GetOrderById), new { orderId }, new { orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerName}.", createOrderDto.CustomerName);
            
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Gets an order by its unique identifier.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <returns>The order details if found; otherwise, Not Found.</returns>
    /// <response code="200">Returns the requested order.</response>
    /// <response code="404">If the order with the specified ID is not found.</response>
    /// <response code="500">If an unexpected error occurs on the server.</response>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderById(Guid orderId)
    {
        _logger.LogInformation("Retrieving order with ID: {OrderId}", orderId);
        
        try
        {
            var orderDto = await GetOrderByIdAsync(orderId);
            
            if (orderDto == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found.", orderId);
                return NotFound();
            }

            return Ok(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order with ID {OrderId}.", orderId);
            
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// Gets a paginated list of orders for a customer, optionally filtered by status.
    /// </summary>
    /// <param name="customerEmail">The email of the customer.</param>
    /// <param name="status">The status of the orders to retrieve (optional).</param>
    /// <param name="pageNumber">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The number of items per page (default is 10, max is 100).</param>
    /// <returns>A paginated list of order summaries.</returns>
    /// <response code="200">Returns the paginated list of orders.</response>
    /// <response code="400">If the request parameters are invalid.</response>
    /// <response code="500">If an unexpected error occurs on the server.</response>
    [HttpGet("customer-orders")]
    [ProducesResponseType(typeof(PagedResultDto<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomerOrdersByStatus(
        [FromQuery] string customerEmail,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation(
            "Retrieving orders for customer {CustomerEmail}, status: {Status}, page: {PageNumber}", 
            customerEmail, status, pageNumber);

        var validationResult = ValidateCustomerOrdersQueryParameters(customerEmail, pageNumber, pageSize);
        if (validationResult != null)
        {
            return validationResult;
        }

        try
        {
            var result = await GetCustomerOrdersAsync(customerEmail, status, pageNumber, pageSize);
            
            _logger.LogInformation(
                "Retrieved {ItemCount} orders for customer {CustomerEmail}", 
                result.Items.Count, customerEmail);
                
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error retrieving orders for customer {CustomerEmail}, status: {Status}", 
                customerEmail, status);
                
            return StatusCode(
                StatusCodes.Status500InternalServerError, 
                "An unexpected error occurred while retrieving orders. Please try again later.");
        }
    }

    /// <summary>
    /// Creates an order by sending a command through the mediator.
    /// </summary>
    /// <param name="requestDto">The order request details.</param>
    /// <returns>The ID of the newly created order.</returns>
    private async Task<Guid> CreateOrderAsync(CreateOrderRequestDto requestDto)
    {
        var command = new CreateOrderCommand(
            requestDto.CustomerName, 
            requestDto.CustomerEmail, 
            requestDto.Items);
            
        var orderId = await _mediator.Send(command);
        
        _logger.LogInformation("Order {OrderId} created successfully.", orderId);
        
        return orderId;
    }

    /// <summary>
    /// Retrieves an order by ID through the mediator.
    /// </summary>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <returns>The order data or null if not found.</returns>
    private async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var query = new GetOrderByIdQuery(orderId);
        var orderDto = await _mediator.Send(query);
        
        if (orderDto != null)
        {
            _logger.LogInformation("Successfully retrieved order {OrderId}.", orderId);
        }
        
        return orderDto;
    }

    /// <summary>
    /// Retrieves customer orders based on the provided filters and pagination settings.
    /// </summary>
    /// <param name="customerEmail">The customer's email address.</param>
    /// <param name="status">The optional order status filter.</param>
    /// <param name="pageNumber">The page number to retrieve.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paged result of order summaries.</returns>
    private async Task<PagedResultDto<OrderSummaryDto>> GetCustomerOrdersAsync(
        string customerEmail, 
        string? status, 
        int pageNumber, 
        int pageSize)
    {
        var query = new GetCustomerOrdersByStatusQuery
        {
            CustomerEmail = customerEmail,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        return await _mediator.Send(query);
    }

    /// <summary>
    /// Validates the parameters for customer order queries.
    /// </summary>
    /// <param name="customerEmail">The customer's email address.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The page size for pagination.</param>
    /// <returns>A BadRequest result if validation fails; otherwise, null.</returns>
    private IActionResult? ValidateCustomerOrdersQueryParameters(
        string customerEmail, 
        int pageNumber, 
        int pageSize)
    {
        if (string.IsNullOrWhiteSpace(customerEmail))
        {
            return BadRequest("Customer email cannot be empty.");
        }
        
        if (pageNumber <= 0)
        {
            return BadRequest("Page number must be greater than zero.");
        }
        
        return pageSize is <= 0 or > MaxPageSize ? BadRequest($"Page size must be between 1 and {MaxPageSize}.") : null;
    }
}