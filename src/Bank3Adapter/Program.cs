using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureAzureNServiceBusEndpoint("Bank3Adapter")
    .Build()
    .Run();