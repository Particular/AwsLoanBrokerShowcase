using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using NServiceBus.Extensions.Logging;
using NServiceBus.Logging;

// Integrate NServiceBus logging with Microsoft.Extensions.Logging
Microsoft.Extensions.Logging.ILoggerFactory extensionsLoggerFactory = new NLogLoggerFactory();
LogManager.UseFactory(new ExtensionsLoggerFactory(extensionsLoggerFactory));

var builder = Host.CreateApplicationBuilder(args);

// TODO: consider moving common endpoint configuration into a shared project
// for use by all endpoints in the system

var endpointConfiguration = new EndpointConfiguration("Client");

var localStackEdgeUrl = "http://localhost:4566";
var emptyLocalStackCredentials = new BasicAWSCredentials("xxx", "xxx");
var sqsConfig = new AmazonSQSConfig() { ServiceURL = localStackEdgeUrl };
var snsConfig = new AmazonSimpleNotificationServiceConfig() { ServiceURL = localStackEdgeUrl };

var transport = new SqsTransport(
    new AmazonSQSClient(emptyLocalStackCredentials, sqsConfig),
    new AmazonSimpleNotificationServiceClient(emptyLocalStackCredentials, snsConfig));
var routingSettings = endpointConfiguration.UseTransport(transport);
routingSettings.RouteToEndpoint(typeof(FindBestLoan), "LoanBroker");
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
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
    switch (k.Key)
    {
        case sendMessageConsoleKey:
            var messageSession = app.Services.GetRequiredService<IMessageSession>();
            var requestId = Guid.NewGuid().ToString()[..8];
            var prospect = new Prospect("Scrooge", "McDuck");
            Console.WriteLine($"Sending FindBestLoan for prospect {prospect.Name} {prospect.Surname}. Request ID: {requestId}");
            await messageSession.Send(new FindBestLoan(requestId, prospect, 10, 1000));
            break;
        case ConsoleKey.Q:
            running = false;
            break;
    }
}

await app.StopAsync();
app.Dispose();

static async Task OnCriticalError(ICriticalErrorContext context, CancellationToken cancellationToken)
{
    // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
    // https://docs.particular.net/nservicebus/hosting/critical-errors
    // and consider setting up service recovery
    // https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
    try
    {
        await context.Stop(cancellationToken);
    }
    finally
    {
        FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
    }
}

static void FailFast(string message, Exception exception)
{
    try
    {
        // TODO: decide what kind of last resort logging is necessary
        // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
        // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
    }
    finally
    {
        Environment.FailFast(message, exception);
    }
}