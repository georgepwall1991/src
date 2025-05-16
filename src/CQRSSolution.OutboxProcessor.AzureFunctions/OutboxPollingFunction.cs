
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CQRSSolution.Application.Interfaces;
using CQRSSolution.Domain.Entities;
using CQRSSolution.Infrastructure.Persistence;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CQRSSolution.OutboxProcessor.AzureFunctions
{
    /// <summary>
    /// Azure Function that processes outbox messages by publishing them to an event bus.
    /// Implements the transactional outbox pattern to ensure reliable event publishing.
    /// </summary>
    public class OutboxPollingFunction
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IEventBusPublisher _eventBusPublisher;
        private readonly ILogger<OutboxPollingFunction> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly int _batchSize;
        private readonly int _maxRetries;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboxPollingFunction"/> class.
        /// </summary>
        /// <param name="dbContext">The database context for accessing outbox messages.</param>
        /// <param name="eventBusPublisher">The publisher for sending events to the event bus.</param>
        /// <param name="logger">The logger for recording function activity.</param>
        /// <param name="jsonSerializerOptions">The JSON serialization options for deserializing message payloads.</param>
        /// <param name="configuration">The configuration for function settings.</param>
        public OutboxPollingFunction(
            ApplicationDbContext dbContext,
            IEventBusPublisher eventBusPublisher,
            ILogger<OutboxPollingFunction> logger,
            JsonSerializerOptions jsonSerializerOptions,
            IConfiguration configuration)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _eventBusPublisher = eventBusPublisher ?? throw new ArgumentNullException(nameof(eventBusPublisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
            
            _batchSize = int.TryParse(configuration["OutboxBatchSize"], out var batchSizeVal) ? batchSizeVal : 10;
            _maxRetries = int.TryParse(configuration["OutboxMaxRetries"], out var maxRetriesVal) ? maxRetriesVal : 5;
        }

        /// <summary>
        /// Executes on a timer schedule to process pending outbox messages.
        /// </summary>
        /// <param name="myTimer">Timer information from the Azure Functions runtime.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [Function("OutboxPollingFunction")]
        public async Task Run([TimerTrigger("%OutboxPollSchedule%")] TimerInfo myTimer)
        {
            _logger.LogInformation("OutboxPollingFunction executed at: {UtcNow}", DateTime.UtcNow);

            var messagesToProcess = await FetchPendingMessagesAsync();
            
            if (!messagesToProcess.Any())
            {
                _logger.LogInformation("No unprocessed outbox messages found within retry limits.");
                return;
            }

            _logger.LogInformation("Found {MessageCount} outbox messages to process.", messagesToProcess.Count);

            foreach (var outboxMessage in messagesToProcess)
            {
                await ProcessOutboxMessageAsync(outboxMessage);
            }

            _logger.LogInformation("OutboxPollingFunction processing cycle finished.");
        }

        /// <summary>
        /// Fetches pending outbox messages that need to be processed.
        /// </summary>
        /// <returns>A list of pending outbox messages.</returns>
        private async Task<List<OutboxMessage>> FetchPendingMessagesAsync()
        {
            return await _dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null && m.Attempts < _maxRetries)
                .OrderBy(m => m.OccurredOnUtc)
                .Take(_batchSize)
                .ToListAsync();
        }

        /// <summary>
        /// Processes a single outbox message by deserializing and publishing it.
        /// </summary>
        /// <param name="outboxMessage">The outbox message to process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ProcessOutboxMessageAsync(OutboxMessage outboxMessage)
        {
            bool publishedSuccessfully = false;
            
            try
            {
                publishedSuccessfully = await TryPublishMessageAsync(outboxMessage);
            }
            catch (Exception ex)
            {
                await HandlePublishExceptionAsync(outboxMessage, ex);
            }
            finally
            {
                await UpdateMessageStateAsync(outboxMessage, publishedSuccessfully);
            }
        }

        /// <summary>
        /// Attempts to publish an outbox message to the event bus.
        /// </summary>
        /// <param name="outboxMessage">The outbox message to publish.</param>
        /// <returns>True if the message was published successfully; otherwise, false.</returns>
        private async Task<bool> TryPublishMessageAsync(OutboxMessage outboxMessage)
        {
            Type? eventType = Type.GetType(outboxMessage.Type);
            
            if (eventType == null)
            {
                _logger.LogError("Could not resolve type {EventTypeFqn} for outbox message ID {OutboxMessageId}.",
                    outboxMessage.Type, outboxMessage.Id);
                outboxMessage.Error = $"Type '{outboxMessage.Type}' not found.";
                return false;
            }

            object? deserializedEvent = JsonSerializer.Deserialize(outboxMessage.Payload, eventType, _jsonSerializerOptions);
            
            if (deserializedEvent == null)
            {
                _logger.LogError("Failed to deserialize payload for outbox message ID {OutboxMessageId}.",
                    outboxMessage.Id);
                outboxMessage.Error = "Payload deserialization failed.";
                return false;
            }

            _logger.LogInformation("Publishing event for message ID: {OutboxMessageId}, Attempt: {AttemptNumber}",
                outboxMessage.Id, outboxMessage.Attempts + 1);

            await _eventBusPublisher.PublishAsync(deserializedEvent, outboxMessage.Type, outboxMessage.Id);
            
            outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            outboxMessage.Error = null;
            
            _logger.LogInformation("Successfully published event for message ID: {OutboxMessageId}.",
                outboxMessage.Id);
                
            return true;
        }

        /// <summary>
        /// Handles exceptions that occur during message publishing.
        /// </summary>
        /// <param name="outboxMessage">The outbox message being processed.</param>
        /// <param name="exception">The exception that occurred.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task HandlePublishExceptionAsync(OutboxMessage outboxMessage, Exception exception)
        {
            _logger.LogError(exception, "Error processing outbox message ID {OutboxMessageId}, Attempt {AttemptNumber}.",
                outboxMessage.Id, outboxMessage.Attempts + 1);
            
            outboxMessage.Error = exception.Message;
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates the state of an outbox message after processing and persists changes.
        /// </summary>
        /// <param name="outboxMessage">The outbox message to update.</param>
        /// <param name="publishedSuccessfully">Whether the message was published successfully.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task UpdateMessageStateAsync(OutboxMessage outboxMessage, bool publishedSuccessfully)
        {
            outboxMessage.Attempts++;
            
            if (outboxMessage.Attempts >= _maxRetries && !publishedSuccessfully)
            {
                _logger.LogWarning("Message ID {OutboxMessageId} has reached max retries ({MaxRetries}).",
                    outboxMessage.Id, _maxRetries);
            }
            
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException dbEx)
            {
                _logger.LogWarning(dbEx, "Concurrency conflict saving message ID {OutboxMessageId}.", 
                    outboxMessage.Id);
                _dbContext.Entry(outboxMessage).State = EntityState.Detached;
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Failed to save changes for message ID {OutboxMessageId}.", 
                    outboxMessage.Id);
            }
        }
    }
}