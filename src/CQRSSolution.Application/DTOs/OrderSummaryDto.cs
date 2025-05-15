namespace CQRSSolution.Application.DTOs;

/// <summary>
///     Represents a summarized view of an order, typically used in lists.
/// </summary>
public class OrderSummaryDto
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}