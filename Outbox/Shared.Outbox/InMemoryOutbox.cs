using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace Shared.Outbox
{
    /// <summary>
    /// In memory implementation of an outbox. 
    /// In memory implementation makes no sense in production code as the entire idea with an outbox is to save messages within the same transaction as the business code.
    /// This implementation is only to show the principles.
    /// </summary>
    internal class InMemoryOutbox : IOutbox, IOutboxProcessor
    {
        private readonly Dictionary<string, Type> _messageTypes = new();
        private readonly ConcurrentQueue<OutboxMessageWrapper> _messages = new();
        private readonly IServiceProvider _services;
        private readonly ILogger<InMemoryOutbox> _logger;

        public InMemoryOutbox(IServiceProvider services, ILogger<InMemoryOutbox> logger)
        {
            _services = services;
            _logger = logger;
        }

        public void Add<TMessage>(TMessage message) where TMessage : IOutboxMessage, new()
        {
            _messages.Enqueue(OutboxMessageWrapper.Create(message));
        }

        public void ProcessOutboxMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _messages.TryPeek(out var wrapper))
            {
                if (!_messageTypes.TryGetValue(wrapper.MessageType, out var type))
                {
                    throw new InvalidOperationException($"Unknown message type: {wrapper.MessageType}");
                }

                var deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var instance = JsonSerializer.Deserialize(wrapper.MessageJson, type, deserializeOptions) as IOutboxMessage
                    ?? throw new ArgumentException($"Invalid IOutboxMessage: {type.FullName}");

                var result = ProcessMessage(instance, wrapper.RetryAttempts);

                if (result.ShouldRetry)
                {
                    if (_messages.TryDequeue(out var w))
                    {
                        w.MarkForRetry(result.RetryDelay ?? TimeSpan.Zero, result.Exception);
                        _messages.Enqueue(w);
                    }
                }
                else
                {
                    _messages.TryDequeue(out var _);
                }
            }
        }

        public void RegisterOutboxMessageType(Type type)
        {
            var instance = Activator.CreateInstance(type) as IOutboxMessage ?? throw new ArgumentException($"Invalid IOutboxMessage: {type.FullName}");
            _messageTypes.Add(instance.MessageType, type);
        }

        private HandlingResult ProcessMessage(IOutboxMessage message, int retryAttempts)
        {
            var scopedServices = _services.CreateScope().ServiceProvider;

            try
            {
                try
                {
                    return message.Process(scopedServices);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation(ex, "Processing of message failed: {@message}, exception: {exception}", message, ex.Message);
                    return message.OnException(ex, retryAttempts, scopedServices);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception while processing outbox message: {@message}, exception: {exception}. Processing will not be retried!", message, ex.Message);
                return HandlingResult.FailureNoRetry(ex);
            }
        }

        private class OutboxMessageWrapper
        {
            public DateTimeOffset ProcessingTime { get; private set; }
            public string MessageType { get; private set; } = "";
            public string MessageJson { get; private set; } = "";
            public int RetryAttempts { get; private set; }
            public Exception? LatestException { get; private set; }

            public static OutboxMessageWrapper Create<TMessage>(TMessage message) where TMessage : IOutboxMessage
            {
                return new OutboxMessageWrapper
                {
                    ProcessingTime = DateTimeOffset.UtcNow,
                    MessageType = message.MessageType,
                    MessageJson = JsonSerializer.Serialize(message)
                };
            }

            public void MarkForRetry(TimeSpan retryDelay, Exception? exception)
            {
                ProcessingTime += retryDelay;
                RetryAttempts++;
                LatestException = exception;
            }
        }
    }
}
