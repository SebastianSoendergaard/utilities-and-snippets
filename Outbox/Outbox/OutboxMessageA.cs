using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Outbox;

namespace Outbox
{
    internal class OutboxMessageA : IOutboxMessage
    {
        public string Input { get; private set; } = "";

        public static OutboxMessageA Create(string input)
        {
            return new OutboxMessageA { Input = input };
        }

        public string MessageType => nameof(OutboxMessageA);

        public HandlingResult Process(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<OutboxMessageA>>();

            logger.LogInformation("Processing {messageType} with text value: {input}", MessageType, Input);

            return HandlingResult.Success();
        }

        public HandlingResult OnException(Exception exception, int retryCount, IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<OutboxMessageA>>();

            logger.LogError("Processing {messageType} failed unexpected with text value: {input}", MessageType, Input);

            return HandlingResult.FailureNoRetry();
        }
    }
}
