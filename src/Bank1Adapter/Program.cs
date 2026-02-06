using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureAwsNServiceBusEndpoint("Bank1Adapter", c => c.Handlers.BankAdapter.AddAll())
    .Build()
    .Run();