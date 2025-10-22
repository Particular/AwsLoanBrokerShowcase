using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureNServiceBusEndpoint("Bank2Adapter")
    .Build()
    .Run();