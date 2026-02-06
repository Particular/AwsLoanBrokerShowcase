using Bank2Adapter;
using CommonConfigurations;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureAwsNServiceBusEndpoint("Bank2Adapter", c => c.Handlers.BankAdapter.AddAll())
    .Build()
    .Run();