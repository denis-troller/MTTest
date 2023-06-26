using Contracts;
using InNova;
using MassTransit;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddMassTransit(x =>
        {
      
            GlobalTopology.Send.UseCorrelationId<TestClosed>(x => x.CorrelationId);

            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: "client", includeNamespace: false));
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
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
