using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Microsoft.Extensions.Hosting;

// To create a docker container, use the following command: dotnet publish /t:PublishContainer
// See https://learn.microsoft.com/en-us/dotnet/core/docker/publish-as-container?pivots=dotnet-8-0#publish-net-app for details

var builder = Host.CreateApplicationBuilder(args);

var endpointConfiguration = new EndpointConfiguration("Bank2Adapter");

var localStackEdgeUrl = "http://localhost:4566";
var emptyLocalStackCredentials = new BasicAWSCredentials("xxx","xxx");
var sqsConfig = new AmazonSQSConfig() { ServiceURL = localStackEdgeUrl };
var snsConfig = new AmazonSimpleNotificationServiceConfig(){ ServiceURL = localStackEdgeUrl };

var transport = new SqsTransport(
    new AmazonSQSClient(emptyLocalStackCredentials, sqsConfig),
    new AmazonSimpleNotificationServiceClient(emptyLocalStackCredentials, snsConfig));
endpointConfiguration.UseTransport(transport);

var persistence = endpointConfiguration.UsePersistence<DynamoPersistence>();
persistence.Sagas().UsePessimisticLocking = true;
persistence.DynamoClient(new AmazonDynamoDBClient(emptyLocalStackCredentials,
    new AmazonDynamoDBConfig() { ServiceURL = localStackEdgeUrl }));
endpointConfiguration.EnableOutbox();
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
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
