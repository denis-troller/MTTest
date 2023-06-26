using Contracts;
using IntegrationService;
using IntegrationService.Integration;
using MassTransit;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(c =>
    {
        c.AddCommandLine(args);
    })
    .ConfigureServices((builder, services) =>
    {
        services.Configure<IntegrationOptions>(builder.Configuration.GetSection("integration"));

        var svcOptions = builder.Configuration.GetSection("integration").Get<IntegrationOptions>();
        var instanceName = string.IsNullOrEmpty(svcOptions.InstanceName) ? "silo1" : svcOptions.InstanceName;
        services.AddMassTransit(x =>
        {
            x.AddDelayedMessageScheduler();

            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: instanceName, includeNamespace: false));

            x.AddConsumer<IntegrateTestJobConsumer, IntegrateTestJobConsumerDefinition>()                
                ;

            x.AddSagaRepository<JobSaga>()
                .InMemoryRepository();
            x.AddSagaRepository<JobTypeSaga>()
                .InMemoryRepository();
            x.AddSagaRepository<JobAttemptSaga>()
                .InMemoryRepository();

            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(prefix: "innova", includeNamespace: false));
            GlobalTopology.Send.UseCorrelationId<TestIntegrationRequest>(x => x.CorrelationId);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.UseDelayedMessageScheduler();


                var options = new ServiceInstanceOptions()
                    .SetEndpointNameFormatter(context.GetService<IEndpointNameFormatter>() ?? KebabCaseEndpointNameFormatter.Instance);

                cfg.ServiceInstance(options, instance =>
                {
                    instance.ConfigureJobServiceEndpoints(js =>
                    {
                        js.SagaPartitionCount = 1;
                        js.FinalizeCompleted = false; // for demo purposes, to get state

                        js.ConfigureSagaRepositories(context);                        
                    });

                    // configure the job consumer on the job service endpoints
                    instance.ConfigureEndpoints(context, f => f.Include<IntegrateTestJobConsumer>());
                });

                cfg.ConfigureEndpoints(context);
            });
        });
    })
    .Build();

await host.RunAsync();
