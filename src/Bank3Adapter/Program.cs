using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureNServiceBusEndpoint("Bank3Adapter")
    .Build()
    .Run();