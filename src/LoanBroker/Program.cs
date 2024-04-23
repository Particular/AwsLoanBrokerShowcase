// using System.Runtime.ExceptionServices;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using LoanBroker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using NServiceBus.Extensions.Logging;
using NServiceBus.Logging;

// Integrate NServiceBus logging with Microsoft.Extensions.Logging
Microsoft.Extensions.Logging.ILoggerFactory extensionsLoggerFactory = new NLogLoggerFactory();
LogManager.UseFactory(new ExtensionsLoggerFactory(extensionsLoggerFactory));
var defaultFactory = LogManager.Use<DefaultFactory>();
defaultFactory.Level(LogLevel.Warn);

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<ICreditScoreProvider, CreditScoreProvider>();
builder.Services.AddSingleton<IQuoteAggregator, BestRateQuoteAggregator>();

var endpointConfiguration = new EndpointConfiguration("LoanBroker");

var localStackEdgeUrl = "http://localhost:4566";
var dummy = new BasicAWSCredentials("xxx","xxx");
var sqsConfig = new AmazonSQSConfig() { ServiceURL = localStackEdgeUrl };
var snsConfig = new AmazonSimpleNotificationServiceConfig(){ ServiceURL = localStackEdgeUrl };

var transport = new SqsTransport(
    new AmazonSQSClient(dummy, sqsConfig),
    new AmazonSimpleNotificationServiceClient(dummy, snsConfig));
endpointConfiguration.UseTransport(transport);

//endpointConfiguration.UsePersistence<DynamoPersistence>();
endpointConfiguration.UsePersistence<LearningPersistence>();
//endpointConfiguration.EnableOutbox();
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();
app.Run();

static async Task OnCriticalError(ICriticalErrorContext context, CancellationToken cancellationToken)
{
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