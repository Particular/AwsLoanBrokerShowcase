using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using NServiceBus.Configuration.AdvancedExtensibility;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CommonConfigurations;

public static class SharedConventions
{
    public const string LocalStackEdgeUrl = "http://localstack:4566";
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

    public static void CommonEndpointSetting(this EndpointConfiguration endpointConfiguration)
    {
        // disable diagnostic writer to prevent docker errors
        // in production each container should map a volume to write diagnostic
        endpointConfiguration.CustomDiagnosticsWriter((_, _) => Task.CompletedTask);
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();
        endpointConfiguration.EnableInstallers();
        EnableMetrics(endpointConfiguration);
        EnableTracing(endpointConfiguration);
    }


    static void EnableMetrics(EndpointConfiguration endpointConfiguration)
    {
        var endpointName = endpointConfiguration.GetSettings().EndpointName();
        var attributes = new Dictionary<string, object>
        {
            ["service.name"] = endpointName,
            ["service.instance.id"] = Guid.NewGuid().ToString(),
        };

        var resourceBuilder = ResourceBuilder.CreateDefault().AddAttributes(attributes);

        Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddMeter("NServiceBus.Core")
            .AddPrometheusHttpListener(options => options.UriPrefixes = new[] { "http://*:9464/" })
            .Build();

        endpointConfiguration.EnableOpenTelemetry();
    }

    static void EnableTracing(EndpointConfiguration endpointConfiguration)
    {
        var endpointName = endpointConfiguration.GetSettings().EndpointName();

        var attributes = new Dictionary<string, object>
        {
            ["service.name"] = endpointName,
            ["service.instance.id"] = Guid.NewGuid().ToString(),
        };

        var resourceBuilder = ResourceBuilder.CreateDefault().AddAttributes(attributes);

        Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resourceBuilder)
            .AddSource("NServiceBus.Core")
            .AddOtlpExporter(cfg =>
            {
                cfg.Endpoint = new Uri("http://jaeger:4318/v1/traces");
                cfg.Protocol = OtlpExportProtocol.HttpProtobuf;
            })
            .Build();

        endpointConfiguration.EnableOpenTelemetry();
    }
}