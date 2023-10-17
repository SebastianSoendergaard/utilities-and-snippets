using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outbox;
using Shared.Outbox;
using System.Reflection;

var host = Host
  .CreateDefaultBuilder()
  .ConfigureServices((context, services) =>
  {
      services.AddInMemoryOutbox();
      services.AddScoped<OutboxTestApp>();

  })
  //.UseOutbox()
  .UseConsoleLifetime()
  .Build();

host.UseOutbox(Assembly.GetExecutingAssembly());
host.Start();

var app = host.Services.GetRequiredService<OutboxTestApp>();
await app.ExecuteAsync();

class OutboxTestApp
{
    private readonly IOutbox _outbox;
    private readonly ILogger<OutboxTestApp> _logger;

    public OutboxTestApp(IOutbox outbox, ILogger<OutboxTestApp> logger)
    {
        _outbox = outbox;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting...");

        while (GetOutboxMessage()) ;

        Console.WriteLine("quiting...");
        _logger.LogInformation("Stopping...");

        await Task.CompletedTask;
    }

    private bool GetOutboxMessage()
    {
        while (true)
        {
            Console.WriteLine("Type: A or B or Q");
            var c = Console.ReadKey().KeyChar;
            if (c == 'A' || c == 'a')
            {
                _outbox.Add(OutboxMessageA.Create(GetInput()));
                return true;
            }
            if (c == 'B' || c == 'b')
            {
                _outbox.Add(OutboxMessageB.Create(GetInput()));
                return true;
            }
            if (c == 'Q' || c == 'q')
            {
                return false;
            }
        }
    }

    private string GetInput()
    {
        Console.WriteLine("Type input value:");
        return Console.ReadLine() ?? "";
    }
}
