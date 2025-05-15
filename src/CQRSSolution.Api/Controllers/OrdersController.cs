using CQRSSolution.Application.Commands.CreateOrder;
using CQRSSolution.Application.DTOs;
using CQRSSolution.Application.Queries.GetCustomerOrderById;
using CQRSSolution.Application.Queries.GetCustomerOrdersByStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CQRSSolution.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    ///     Creates a new order.
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
        if (!ModelState.IsValid) // Basic validation, FluentValidation can be added later
        {
            _logger.LogWarning("CreateOrder called with invalid model state.");
            return BadRequest(ModelState);
        }

        try
        {
            var command = new CreateOrderCommand(createOrderDto.CustomerName, createOrderDto.CustomerEmail, createOrderDto.Items);
            
            var orderId = await _mediator.Send(command);

            _logger.LogInformation("Order {OrderId} created successfully through API.", orderId);

            return CreatedAtAction(nameof(GetOrderById), new { orderId }, new { orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating order for customer {CustomerName}.", createOrderDto.CustomerName);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    ///     Gets an order by its ID.
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
        _logger.LogInformation("GetOrderById called for OrderId: {OrderId}", orderId);
        try
        {
            var query = new GetOrderByIdQuery(orderId);
            var orderDto = await _mediator.Send(query);

            if (orderDto == null)
            {
                _logger.LogWarning("GetOrderById: Order with ID {OrderId} not found.", orderId);
                return NotFound();
            }

            _logger.LogInformation("GetOrderById: Successfully retrieved order {OrderId}.", orderId);
            return Ok(orderDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving order {OrderId}.", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    ///     Gets a paginated list of orders for a customer, filtered by status.
    /// </summary>
    /// <param name="customerEmail">The email of the customer.</param>
    /// <param name="status">The status of the orders to retrieve.</param>
    /// <param name="pageNumber">The page number for pagination (default is 1).</param>
    /// <param name="pageSize">The number of items per page (default is 10).</param>
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
        _logger.LogInformation("GetCustomerOrdersByStatus called for CustomerEmail: {CustomerEmail}, Status: {Status}, Page: {PageNumber}, PageSize: {PageSize}", customerEmail, status, pageNumber, pageSize);

        if (string.IsNullOrWhiteSpace(customerEmail)) return BadRequest("Customer email cannot be empty.");
        if (pageNumber <= 0) return BadRequest("Page number must be greater than zero.");
        if (pageSize is <= 0 or > 100) // Max page size limit
            return BadRequest("Page size must be between 1 and 100.");

        try
        {
            var query = new GetCustomerOrdersByStatusQuery
            {
                CustomerEmail = customerEmail,
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            var result = await _mediator.Send(query);

            _logger.LogInformation("Successfully retrieved {ItemCount} orders for customer {CustomerEmail}, status {Status}, page {PageNumber}.", result.Items.Count, customerEmail, status, pageNumber);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving orders for customer {CustomerEmail}, status {Status}.", customerEmail, status);
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while retrieving orders. Please try again later.");
        }
    }
}