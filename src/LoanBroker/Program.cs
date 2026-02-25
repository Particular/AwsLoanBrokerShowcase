using CommonConfigurations;
using LoanBroker;
using LoanBroker.Messages;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureServices()
    .ConfigureAwsNServiceBusEndpoint("LoanBroker", c =>
    {
        c.Handlers.LoanBroker.AddAll();
        c.Routing.RouteToEndpoint(typeof(FindBestLoanWithScore), "LoanBroker");
    })
    .Build()
    .Run();