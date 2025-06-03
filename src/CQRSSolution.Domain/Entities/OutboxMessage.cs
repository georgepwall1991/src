using System;
using System.ComponentModel.DataAnnotations;

namespace CQRSSolution.Domain.Entities
{
    /// <summary>
    /// Represents a message to be published, stored temporarily in the outbox for guaranteed delivery.
    /// </summary>
    public class OutboxMessage
    {
        /// <summary>
        /// Gets or sets the unique identifier for the outbox message.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name of the event type.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the serialized event data (e.g., in JSON format).
        /// </summary>
        [Required]
        public string Payload { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Coordinated Universal Time (UTC) when the event occurred.
        /// </summary>
        public DateTime OccurredOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the Coordinated Universal Time (UTC) when the message was processed and published.
        /// Null if the message has not yet been processed.
        /// </summary>
        public DateTime? ProcessedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets an error message if processing failed.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboxMessage"/> class.
        /// </summary>
        public OutboxMessage()
        {
            Id = Guid.NewGuid();
            OccurredOnUtc = DateTime.UtcNow;
        }
    }
} 