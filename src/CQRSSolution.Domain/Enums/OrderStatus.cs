namespace CQRSSolution.Domain.Enums;

/// <summary>
/// Represents the status of an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order is pending confirmation.
    /// </summary>
    Pending,

    /// <summary>
    /// Order has been confirmed.
    /// </summary>
    Confirmed,

    /// <summary>
    /// Order has been shipped.
    /// </summary>
    Shipped,

    /// <summary>
    /// Order has been delivered.
    /// </summary>
    Delivered,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// Order processing failed.
    /// </summary>
    Failed
} 