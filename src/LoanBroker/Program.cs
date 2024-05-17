using Amazon.DynamoDBv2;
using CommonConfigurations;
using LoanBroker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

SharedConventions.ConfigureMicrosoftLoggingIntegration();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<ICreditScoreProvider, CacheCreditScoreProvider>(_ =>
    new CacheCreditScoreProvider(new RandomCreditScoreProvider()));
builder.Services.AddSingleton<IQuoteAggregator, BestRateQuoteAggregator>();

var endpointConfiguration = new EndpointConfiguration("LoanBroker");
endpointConfiguration.CommonEndpointSetting();
endpointConfiguration.UseCommonTransport();

var persistence = endpointConfiguration.UsePersistence<DynamoPersistence>();
persistence.Sagas().UsePessimisticLocking = true;
persistence.DynamoClient(new AmazonDynamoDBClient(SharedConventions.EmptyLocalStackCredentials,
    new AmazonDynamoDBConfig { ServiceURL = SharedConventions.LocalStackUrl() }));

endpointConfiguration.EnableOutbox();

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();
app.Run();