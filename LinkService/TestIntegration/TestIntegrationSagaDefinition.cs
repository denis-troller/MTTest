using Contracts;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkService.TestIntegration
{
    internal class TestIntegrationSagaDefinition : SagaDefinition<TestIntegration>
    {
        public TestIntegrationSagaDefinition()
        {
            // specify the message limit at the endpoint level, which influences
            // the endpoint prefetch count, if supported
            Endpoint(e => e.ConcurrentMessageLimit = 16);
        }

        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<TestIntegration> sagaConfigurator)
        {
            var partition = endpointConfigurator.CreatePartitioner(16);

            sagaConfigurator.Message<TestClosed>(x => x.UsePartitioner(partition, m => m.Message.Id));
            sagaConfigurator.Message<TestIntegrationStarted>(x => x.UsePartitioner(partition, m => m.Message.TestId));
            sagaConfigurator.Message<TestIntegrated>(x => x.UsePartitioner(partition, m => m.Message.TestId));
            sagaConfigurator.Message<TestRejected>(x => x.UsePartitioner(partition, m => m.Message.TestId));
        }
    }
}
