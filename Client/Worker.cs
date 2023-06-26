using Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InNova;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBus _bus;
    public Worker(ILogger<Worker> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {      
        for(var i=0; i<5; i++)
        {

            var testId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            _logger.LogInformation("Publish TestClosed event for test {id}", testId);
            await _bus.Publish(new TestClosed(testId, correlationId));
        }
    }
}
