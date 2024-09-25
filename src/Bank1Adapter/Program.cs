using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureAwsNServiceBusEndpoint("Bank1Adapter")
    .Build()
    .Run();