using NServiceBus.Configuration.AdvancedExtensibility;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace CommonConfigurations;

static class OpenTelemetryExtensions
{
    public static void EnableOpenTelemetryMetrics(this EndpointConfiguration endpointConfiguration)
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
            .AddMeter("NServiceBus.Core.Pipeline.Incoming")
            .AddMeter("LoanBroker")
            .AddOtlpExporter(cfg =>
            {
                var url = Environment.GetEnvironmentVariable(OtlpMetricsUrlEnvVar) ?? OtlpMetricsDefaultUrl;
                cfg.Endpoint = new Uri(url);
                cfg.Protocol = OtlpExportProtocol.HttpProtobuf;
            })
            .Build();

        endpointConfiguration.EnableOpenTelemetry();
    }

    public static void EnableOpenTelemetryTracing(this EndpointConfiguration endpointConfiguration)
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
                var url = Environment.GetEnvironmentVariable(OtlpTracesUrlEnvVar) ?? OtlpTracesDefaultUrl;
                cfg.Endpoint = new Uri(url);
                cfg.Protocol = OtlpExportProtocol.HttpProtobuf;
            })
            .Build();

        endpointConfiguration.EnableOpenTelemetry();

        endpointConfiguration.EnableOpenTelemetry();
    }

    const string OtlpMetricsDefaultUrl = "http://localhost:5318/v1/metrics";
    const string OtlpTracesDefaultUrl = "http://localhost:4318/v1/traces";
    const string OtlpMetricsUrlEnvVar = "OTLP_METRICS_URL";
    const string OtlpTracesUrlEnvVar = "OTLP_TRACING_URL";

}
