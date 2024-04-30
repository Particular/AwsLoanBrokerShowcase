// using System.Runtime.ExceptionServices;

using Amazon.DynamoDBv2;
using Amazon.Runtime;
using CommonConfigurations;
using LoanBroker.Services;
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
builder.Services.AddSingleton<ICreditScoreProvider, CacheCreditScoreProvider>(_ =>
    new CacheCreditScoreProvider(new RandomCreditScoreProvider()));
builder.Services.AddSingleton<IQuoteAggregator, BestRateQuoteAggregator>();

var endpointConfiguration = new EndpointConfiguration("LoanBroker");

endpointConfiguration.UseCommonTransport();

var persistence = endpointConfiguration.UsePersistence<DynamoPersistence>();
persistence.Sagas().UsePessimisticLocking = true;
const string localStackEdgeUrl = "http://localhost:4566";
var emptyLocalStackCredentials = new BasicAWSCredentials("xxx", "xxx");
persistence.DynamoClient(new AmazonDynamoDBClient(emptyLocalStackCredentials,
    new AmazonDynamoDBConfig { ServiceURL = localStackEdgeUrl }));
endpointConfiguration.EnableOutbox();
endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();
app.Run();
return;

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