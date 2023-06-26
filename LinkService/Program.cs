using Contracts;
using LinkService.TestIntegration;
using MassTransit;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: "client", includeNamespace: false));
            x.AddSagaStateMachine<TestIntegrationStateMachine, TestIntegration>()
            .InMemoryRepository();

            GlobalTopology.Send.UseCorrelationId<TestClosed>(x => x.CorrelationId);
            GlobalTopology.Send.UseCorrelationId<TestIntegrationStarted>(x => x.CorrelationId);
            GlobalTopology.Send.UseCorrelationId<TestIntegrated>(x => x.CorrelationId);
            GlobalTopology.Send.UseCorrelationId<TestRejected>(x => x.CorrelationId);

            x.AddSagas(typeof(TestIntegration).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

await host.RunAsync();
