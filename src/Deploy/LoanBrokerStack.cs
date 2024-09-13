using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.SQS;
using BankMessages;
using Deploy;

class LoanBrokerStack : Stack
{
    public LoanBrokerStack(Construct scope, string id, IStackProps? props = null)
        : base(scope, id, props)
    {
        _ = new NServiceBusEndpointResource(new EndpointDetails("LoanBroker") { EnableDynamoDBPersistence = true },
            this, "LoanBroker.LoanBroker");
        _ = new NServiceBusEndpointResource(new EndpointDetails("Client"), this, "LoanBroker.Client");
        var endpointDetails = new EndpointDetails("Bank1Adapter");
        endpointDetails.EventsToSubscribe = [typeof(QuoteRequested)];
        _ = new NServiceBusEndpointResource(endpointDetails, this, "LoanBroker.Bank1Adapter");
        _ = new NServiceBusEndpointResource(new EndpointDetails("Bank2Adapter"), this, "LoanBroker.Bank2Adapter");
        _ = new NServiceBusEndpointResource(new EndpointDetails("Bank3Adapter"), this, "LoanBroker.Bank3Adapter");

        _ = new Queue(this, "error", new QueueProps
        {
            QueueName = "error",
            RetentionPeriod = Duration.Seconds(900)
        });

        _ = new ApplicationLoadBalancedFargateService(this, "Grafana",
            new ApplicationLoadBalancedFargateServiceProps
            {
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {
                    Image = ContainerImage.FromRegistry("grafana/grafana-oss:latest"),
                    ContainerPort = 3000,
                },
                PublicLoadBalancer = true
            });

        var prometheus = new ApplicationLoadBalancedFargateService(this, "Prometheus",
            new ApplicationLoadBalancedFargateServiceProps
            {
                TaskImageOptions = new ApplicationLoadBalancedTaskImageOptions
                {

                    Image = ContainerImage.FromRegistry("prom/prometheus:v2.53.2"),
                    ContainerPort = 9090
                },
                PublicLoadBalancer = true

            });
        var cfnTaskDefinition = (CfnTaskDefinition)prometheus.TaskDefinition.Node.DefaultChild;
        cfnTaskDefinition.AddOverride("properties.containerDefinitions.0.command", "--web.enable-lifecycle");

        // // grafana:
        // // TODO: restart: unless-stopped ---> policy from AWS UI
        // // volumes:
        // // - ./grafana/provisioning:/etc/grafana/provisioning/
        // //     - ./grafana/dashboards:/var/lib/grafana/dashboards
        // //     - ./volumes/grafana-data:/var/lib/grafana
        // // networks:
        // // - ls

    }
}