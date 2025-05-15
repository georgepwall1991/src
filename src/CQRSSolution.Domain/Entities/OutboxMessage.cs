namespace CQRSSolution.Domain.Entities;

/// <summary>
///     Represents a message stored in the outbox for eventual processing and publishing.
/// </summary>
public class OutboxMessage
{
    /// <summary>
    ///     Gets or sets the unique identifier for the outbox message.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the fully qualified name of the event type.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the serialized payload of the event data.
    /// </summary>
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the date and time when the event occurred (UTC).
    /// </summary>
    public DateTime OccurredOnUtc { get; set; }

    /// <summary>
    ///     Gets or sets the date and time when the message was processed (UTC).
    ///     Null if not yet processed.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    ///     Gets or sets an error message if processing failed.
    /// </summary>
    public string? Error { get; set; }
}