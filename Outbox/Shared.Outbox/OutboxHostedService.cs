using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Outbox
{
    internal class OutboxHostedService : BackgroundService
    {
        private readonly IOutboxProcessor _processor;
        private readonly ILogger<OutboxHostedService> _logger;

        public OutboxHostedService(IOutboxProcessor processor, ILogger<OutboxHostedService> logger)
        {
            _processor = processor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Outbox starting...");

            while (!cancellationToken.IsCancellationRequested)
            {
                _processor.ProcessOutboxMessages(cancellationToken);
                await Task.Delay(1000, cancellationToken);
            }

            _logger.LogInformation("Outbox stopping...");
        }


    }
}
