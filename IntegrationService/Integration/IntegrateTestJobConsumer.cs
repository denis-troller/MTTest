using Contracts;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationService.Integration
{
    internal class IntegrateTestJobConsumer : IJobConsumer<TestIntegrationRequest>
    {
        readonly ILogger<IntegrateTestJobConsumer> _logger;

        public IntegrateTestJobConsumer(ILogger<IntegrateTestJobConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Run(JobContext<TestIntegrationRequest> context)
        {
            var rng = new Random();

            var variance = TimeSpan.FromMilliseconds(rng.Next(2000, 5000));

            _logger.LogInformation("Integrating test: {id}", context.Job.TestId);

            await context.Publish(new TestIntegrationStarted(context.Job.TestId, context.Job.CorrelationId));
            await Task.Delay(variance);

            var OK = rng.Next(1, 10) >= 5;

            if (OK)
            { 
                _logger.LogInformation("Integrated test: {id}", context.Job.TestId);
                await context.Publish(new TestIntegrated(context.Job.TestId, context.Job.CorrelationId));
            }
            else
            {
                _logger.LogError("Integration for test {id} failed", context.Job.TestId);
                await context.Publish(new TestRejected(context.Job.TestId, context.Job.CorrelationId));
            }

        }
    }

    internal class IntegrateTestJobConsumerDefinition : ConsumerDefinition<IntegrateTestJobConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
       IConsumerConfigurator<IntegrateTestJobConsumer> consumerConfigurator)
        {
            consumerConfigurator.Options<JobOptions<TestIntegrationRequest>>(options =>
                options
                    .SetRetry(r => r.Interval(3, TimeSpan.FromSeconds(30)))
                    .SetJobTimeout(TimeSpan.FromMinutes(10))
                    .SetConcurrentJobLimit(10));
        }
    }
}
