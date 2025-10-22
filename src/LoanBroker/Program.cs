using CommonConfigurations;
using LoanBroker;
using LoanBroker.Messages;
using Microsoft.Extensions.Hosting;

Host.CreateApplicationBuilder(args)
    .ConfigureServices()
    .ConfigureNServiceBusEndpoint("LoanBroker", c =>
    {
        c.Persistence.Sagas().UsePessimisticLocking();

        c.Routing.RouteToEndpoint(typeof(FindBestLoanWithScore), "LoanBroker");
    })
    .Build()
    .Run();