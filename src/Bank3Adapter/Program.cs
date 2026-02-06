using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureAwsNServiceBusEndpoint("Bank3Adapter", c => c.Handlers.BankAdapter.AddAll())
    .Build()
    .Run();