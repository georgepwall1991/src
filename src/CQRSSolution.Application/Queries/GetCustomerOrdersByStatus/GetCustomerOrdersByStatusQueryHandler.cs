using CQRSSolution.Application.DTOs;
using CQRSSolution.Application.Interfaces;
using CQRSSolution.Application.Specifications.Orders;
using CQRSSolution.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CQRSSolution.Application.Queries.GetCustomerOrdersByStatus;

/// <summary>
///     Handles the <see cref="GetCustomerOrdersByStatusQuery" /> to retrieve a paginated list of orders.
/// </summary>
public class
    GetCustomerOrdersByStatusQueryHandler : IRequestHandler<GetCustomerOrdersByStatusQuery,
    PagedResultDto<OrderSummaryDto>>
{
    private readonly ILogger<GetCustomerOrdersByStatusQueryHandler> _logger;
    private readonly IRepository<Order> _orderRepository;

    public GetCustomerOrdersByStatusQueryHandler(IRepository<Order> orderRepository,
        ILogger<GetCustomerOrdersByStatusQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<PagedResultDto<OrderSummaryDto>> Handle(GetCustomerOrdersByStatusQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling GetCustomerOrdersByStatusQuery for CustomerEmail: {CustomerEmail}, Status: {Status}, Page: {PageNumber}, PageSize: {PageSize}",
            request.CustomerEmail, request.Status, request.PageNumber, request.PageSize);

        var spec = new CustomerOrdersByStatusSpecification(
            request.CustomerEmail,
            request.Status,
            request.PageNumber,
            request.PageSize);

        // Fetch the items for the current page
        var orders = await _orderRepository.ListAsync(spec, cancellationToken);

        // To get the total count, we need a specification without pagination applied for the count query.
        // Create a new specification instance for counting, or modify the existing one if it allows clearing pagination.
        // For simplicity here, we'll create a new one for count without pagination.
        // A more advanced BaseSpecification could have a method like `ClearPaging()` or `AsCountSpecification()`.
        var countSpec = new CustomerOrdersByStatusSpecification(request.CustomerEmail, request.Status, 1, int.MaxValue);
        // Temporarily remove pagination for counting. A better way would be to have a separate spec or a flag in ApplySpecification.
        countSpec.GetType().GetProperty("IsPagingEnabled")?.SetValue(countSpec, false);

        var totalCount = await _orderRepository.CountAsync(countSpec, cancellationToken);

        var orderSummaries = orders.Select(order => new OrderSummaryDto
        {
            OrderId = order.OrderId,
            CustomerName = order.Customer?.Name ?? "N/A", // Handle potential null Customer
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ItemCount = order.OrderItems?.Count ?? 0 // Handle potential null OrderItems
        }).ToList();

        _logger.LogInformation("Retrieved {ItemCount} orders for page {PageNumber} with total count {TotalCount}",
            orderSummaries.Count, request.PageNumber, totalCount);

        return new PagedResultDto<OrderSummaryDto>(orderSummaries, totalCount, request.PageNumber, request.PageSize);
    }
}