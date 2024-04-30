using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;

namespace CommonConfigurations;

public static class TransportConventions
{
    private const string LocalStackEdgeUrl = "http://localhost:4566";

    public static RoutingSettings UseCommonTransport(this EndpointConfiguration endpointConfiguration)
    {
        var emptyLocalStackCredentials = new BasicAWSCredentials("xxx", "xxx");
        var sqsConfig = new AmazonSQSConfig { ServiceURL = LocalStackEdgeUrl };
        var snsConfig = new AmazonSimpleNotificationServiceConfig { ServiceURL = LocalStackEdgeUrl };

        var transport = new SqsTransport(
            new AmazonSQSClient(emptyLocalStackCredentials, sqsConfig),
            new AmazonSimpleNotificationServiceClient(emptyLocalStackCredentials, snsConfig));
        return endpointConfiguration.UseTransport(transport);
    }
}