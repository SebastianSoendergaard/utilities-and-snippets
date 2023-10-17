namespace Shared.Outbox
{
    public interface IOutbox
    {
        void Add<TMessage>(TMessage message) where TMessage : IOutboxMessage, new();
    }

    public interface IOutboxMessage
    {
        string MessageType { get; }
        HandlingResult Process(IServiceProvider services);
        HandlingResult OnException(Exception exception, int retryCount, IServiceProvider services);
    }

    public class HandlingResult
    {
        public bool ShouldRetry { get; private set; }
        public Exception? Exception { get; private set; }
        public TimeSpan? RetryDelay { get; private set; }

        public static HandlingResult Success()
        {
            return new HandlingResult { ShouldRetry = false };
        }

        public static HandlingResult FailureNoRetry(Exception? exception = null)
        {
            return new HandlingResult { ShouldRetry = false, Exception = exception };
        }

        public static HandlingResult Retry(Exception? exception = null, TimeSpan? retryDelay = null)
        {
            return new HandlingResult { ShouldRetry = true, Exception = exception, RetryDelay = retryDelay };
        }
    }
}
