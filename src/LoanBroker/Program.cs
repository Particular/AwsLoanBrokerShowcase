using CommonConfigurations;
using LoanBroker;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureServices()
    .ConfigureNServiceBusEndpoint("LoanBroker", c => { c.ApplyLoanBrokerEndpointCustomizations(); })
    .Build()
    .Run();