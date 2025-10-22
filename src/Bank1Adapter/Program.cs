using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureNServiceBusEndpoint("Bank1Adapter")
    .Build()
    .Run();