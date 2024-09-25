using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureAwsNServiceBusEndpoint("Bank2Adapter")
    .Build()
    .Run();