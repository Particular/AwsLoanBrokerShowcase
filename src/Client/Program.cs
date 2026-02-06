using Client;
using ClientMessages;
using CommonConfigurations;
using Microsoft.Extensions.Hosting;

var app = Host.CreateApplicationBuilder(args)
    .ConfigureAwsNServiceBusEndpoint("Client", c =>
    {
        c.Handlers.Client.AddAll();
        c.Routing.RouteToEndpoint(typeof(FindBestLoan), "LoanBroker");
    })
    .Build();

await app.StartAsync();

await UILoop.RunLoop(app, args);

await app.StopAsync();
app.Dispose();
return;

