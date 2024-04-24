using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;

namespace CommonConfigurations;

public static class TransportConventions
{
    public static RoutingSettings UseCommonTransport(
        this EndpointConfiguration endpointConfiguration)
    {
        var localStackEdgeUrl = "http://localhost:4566";
        var emptyLocalStackCredentials = new BasicAWSCredentials("xxx", "xxx");
        var sqsConfig = new AmazonSQSConfig() { ServiceURL = localStackEdgeUrl };
        var snsConfig = new AmazonSimpleNotificationServiceConfig() { ServiceURL = localStackEdgeUrl };

        var transport = new SqsTransport(
            new AmazonSQSClient(emptyLocalStackCredentials, sqsConfig),
            new AmazonSimpleNotificationServiceClient(emptyLocalStackCredentials, snsConfig));
        return endpointConfiguration.UseTransport(transport);
    }
}