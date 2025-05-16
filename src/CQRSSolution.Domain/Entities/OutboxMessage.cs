using System;

namespace CQRSSolution.Domain.Entities
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } // Full type name of the event
        public string Payload { get; set; } // Serialized event data
        public DateTime OccurredOnUtc { get; set; }
        public DateTime? ProcessedOnUtc { get; set; }
        public int Attempts { get; set; } // To track retry attempts
        public string? Error { get; set; } // To store the last error message
    }
} 