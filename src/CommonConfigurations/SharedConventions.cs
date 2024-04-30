using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;

namespace CommonConfigurations;

public static class SharedConventions
{
    public const string LocalStackEdgeUrl = "http://localhost:4566";
    public static readonly AWSCredentials EmptyLocalStackCredentials = new BasicAWSCredentials("xxx", "xxx");

    public static RoutingSettings UseCommonTransport(this EndpointConfiguration endpointConfiguration)
    {

        var sqsConfig = new AmazonSQSConfig { ServiceURL = LocalStackEdgeUrl };
        var snsConfig = new AmazonSimpleNotificationServiceConfig { ServiceURL = LocalStackEdgeUrl };

        var transport = new SqsTransport(
            new AmazonSQSClient(EmptyLocalStackCredentials, sqsConfig),
            new AmazonSimpleNotificationServiceClient(EmptyLocalStackCredentials, snsConfig));
        return endpointConfiguration.UseTransport(transport);
    }
}