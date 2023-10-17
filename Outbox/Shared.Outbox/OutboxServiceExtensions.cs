using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Shared.Outbox
{
    public static class OutboxServiceExtensions
    {
        public static IServiceCollection AddInMemoryOutbox(this IServiceCollection services)
        {
            return services.AddOutbox<InMemoryOutbox>();
        }

        private static IServiceCollection AddOutbox<TOutbox>(this IServiceCollection services) where TOutbox : class, IOutbox, IOutboxProcessor
        {
            services.AddHostedService<OutboxHostedService>();

            services.AddSingleton<TOutbox>();
            services.AddSingleton<IOutbox>(s => s.GetRequiredService<TOutbox>());
            services.AddSingleton<IOutboxProcessor>(s => s.GetRequiredService<TOutbox>());

            return services;
        }

        public static IHost UseOutbox(this IHost host, params Assembly[] assemblies)
        {
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("Outbox");
            logger.LogInformation("Registring outbox message types...");

            var processor = host.Services.GetRequiredService<IOutboxProcessor>();

            foreach (var type in SearchOutboxMessages(assemblies))
            {
                processor.RegisterOutboxMessageType(type);
            }

            return host;
        }

        private static IEnumerable<Type> SearchOutboxMessages(Assembly[] assemblies)
        {
            var type = typeof(IOutboxMessage);
            return assemblies
                .SelectMany(s => s.GetTypes())
                .Where(type.IsAssignableFrom);
        }
    }
}
