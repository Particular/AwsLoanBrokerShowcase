using Microsoft.Extensions.Hosting;
using NServiceBus.Logging;


var defaultFactory = LogManager.Use<DefaultFactory>();
defaultFactory.Level(LogLevel.Warn);

// To create a docker container, use the following command: dotnet publish /t:PublishContainer
// See https://learn.microsoft.com/en-us/dotnet/core/docker/publish-as-container?pivots=dotnet-8-0#publish-net-app for details
var builder = Host.CreateApplicationBuilder(args);

// TODO: consider moving common endpoint configuration into a shared project
// for use by all endpoints in the system

var endpointConfiguration = new EndpointConfiguration("LoanBroker");

// SQL Server Transport: https://docs.particular.net/transports/sql/
var transport = new SqlServerTransport("Data Source=.\\SqlExpress;Initial Catalog=dbname;Integrated Security=True");
// var routing = endpointConfiguration.UseTransport(transport);
var routing = endpointConfiguration.UseTransport<LearningTransport>();

// Define routing for commands: https://docs.particular.net/nservicebus/messaging/routing#command-routing
// routing.RouteToEndpoint(typeof(MessageType), "DestinationEndpointForType");
// routing.RouteToEndpoint(typeof(MessageType).Assembly, "DestinationForAllCommandsInAssembly");

// Amazon DynamoDB Persistence: https://docs.particular.net/persistence/dynamodb/
var persistence = endpointConfiguration.UsePersistence<DynamoPersistence>();

// Message serialization
endpointConfiguration.UseSerialization<SystemJsonSerializer>();

endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

// Installers are useful in development. Consider disabling in production.
// https://docs.particular.net/nservicebus/operations/installers
endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();
app.Run();

static async Task OnCriticalError(ICriticalErrorContext context, CancellationToken cancellationToken)
{
    // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
    // https://docs.particular.net/nservicebus/hosting/critical-errors
    // and consider setting up service recovery
    // https://docs.particular.net/nservicebus/hosting/windows-service#installation-restart-recovery
    try
    {
        // await context.Stop(cancellationToken);
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