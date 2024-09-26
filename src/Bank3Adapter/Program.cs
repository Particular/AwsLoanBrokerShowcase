using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureAwsNServiceBusEndpoint("Bank3Adapter")
    .Build()
    .Run();