using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Outbox;

namespace Outbox
{
    internal class OutboxMessageB : IOutboxMessage
    {
        public string Input { get; private set; } = "";

        public static OutboxMessageB Create(string input)
        {
            return new OutboxMessageB { Input = input };
        }

        public string MessageType => nameof(OutboxMessageB);

        public HandlingResult Process(IServiceProvider services)
        {
            var logger = GetLogger(services);

            logger.LogInformation("Processing {messageType} with text value: {input}", MessageType, Input);

            var intValue = int.Parse(Input);

            logger.LogInformation("- {messageType} text value is converted with success into an int value: {intValue}", MessageType, intValue);

            return HandlingResult.Success();
        }

        public HandlingResult OnException(Exception exception, int retryCount, IServiceProvider services)
        {
            // TODO: Fix logging, for some reason logging fails here while it works in Process method

            //var logger = GetLogger(services);

            //logger.LogError("Processing {messageType} failed with text value: {input}, retryCount: {retryCount}, exception: {message}", MessageType, Input, retryCount, exception.Message);

            return retryCount > 10
                ? HandlingResult.FailureNoRetry()
                : HandlingResult.Retry(exception, TimeSpan.FromSeconds(retryCount * retryCount));
        }

        private ILogger<OutboxMessageB> _logger;
        private ILogger<OutboxMessageB> GetLogger(IServiceProvider services)
        {
            return _logger ??= services.GetRequiredService<ILogger<OutboxMessageB>>();
        }
    }
}
