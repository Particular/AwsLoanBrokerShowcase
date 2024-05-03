using ClientMessages;
using CommonConfigurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using NServiceBus.Extensions.Logging;
using NServiceBus.Logging;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

// Integrate NServiceBus logging with Microsoft.Extensions.Logging
ILoggerFactory extensionsLoggerFactory = new NLogLoggerFactory();
LogManager.UseFactory(new ExtensionsLoggerFactory(extensionsLoggerFactory));

var builder = Host.CreateApplicationBuilder(args);

// TODO: consider moving common endpoint configuration into a shared project
// for use by all endpoints in the system

var endpointConfiguration = new EndpointConfiguration("Client");

var routingSettings = endpointConfiguration.UseCommonTransport();
routingSettings.RouteToEndpoint(typeof(FindBestLoan), "LoanBroker");
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();

await app.StartAsync();

const ConsoleKey sendMessageConsoleKey = ConsoleKey.F;
Console.WriteLine($"Press {sendMessageConsoleKey} to send a new FindBestLoan request");
Console.WriteLine("Press Q to quit");

var running = true;
while (running)
{
    var k = Console.ReadKey();
    Console.WriteLine();
    switch (k.Key)
    {
        case sendMessageConsoleKey:
            var messageSession = app.Services.GetRequiredService<IMessageSession>();
            var requestId = Guid.NewGuid().ToString()[..8];
            var prospect = new Prospect("Scrooge", "McDuck");
            Console.WriteLine(
                $"Sending FindBestLoan for prospect {prospect.Name} {prospect.Surname}. Request ID: {requestId}");
            await messageSession.Send(new FindBestLoan(requestId, prospect, 10, 1000));
            break;
        case ConsoleKey.Q:
            running = false;
            break;
    }
}

await app.StopAsync();
app.Dispose();