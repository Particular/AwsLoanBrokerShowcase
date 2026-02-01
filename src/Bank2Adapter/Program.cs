using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureAzureNServiceBusEndpoint("Bank2Adapter")
    .Build()
    .Run();