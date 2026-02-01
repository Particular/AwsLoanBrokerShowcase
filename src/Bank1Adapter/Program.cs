using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureAzureNServiceBusEndpoint("Bank1Adapter")
    .Build()
    .Run();