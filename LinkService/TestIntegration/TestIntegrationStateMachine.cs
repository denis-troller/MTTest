using Contracts;
using MassTransit;
using MassTransit.Futures.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkService.TestIntegration
{
    internal class TestIntegrationStateMachine : MassTransitStateMachine<TestIntegration>
    {
        public TestIntegrationStateMachine(ILogger<TestIntegrationStateMachine> logger)
        {
            InstanceState(x => x.CurrentState, Submitted, Accepted);

            Event(() => TestClosed, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => TestIntegrationStarted, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => TestIntegrated, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => TestRejected, x => x.CorrelateById(context => context.Message.CorrelationId));

            Initially( 
                When(TestClosed)
                .Then((c) => logger.LogInformation("Test Closed message received for {testId}", c.Message.Id))
                .Send(new Uri("exchange:silo2-integrate-test-job"), c => new TestIntegrationRequest(c.Message.Id, c.Message.CorrelationId)                    
                )
                .TransitionTo(Submitted)
            );

            During(Submitted,
                When(TestIntegrationStarted)
                .Then(c => logger.LogInformation("Integration started for test {id}", c.Message.TestId))
                .TransitionTo(Accepted)
            );

            During(Accepted,
                When(TestIntegrated)
                .Then(c => logger.LogInformation("Integration succeeded for test {id}", c.Message.TestId))
                .Finalize()
            );

            During(Accepted,
                When(TestRejected)
                .Then(c => logger.LogError("Integration failed for test {id}", c.Message.TestId))
                .Finalize()
            );

            SetCompletedWhenFinalized();
        }

        public State Accepted { get; private set; } = null!;
        public State Submitted { get; private set; } = null!;

        public Event<TestClosed> TestClosed { get; set; } = null!;
        public Event<TestIntegrationStarted> TestIntegrationStarted { get; set; } = null!;
        public Event<TestIntegrated> TestIntegrated { get; set; } = null!;
        public Event<TestRejected> TestRejected { get; set; } = null!;
    }
}
