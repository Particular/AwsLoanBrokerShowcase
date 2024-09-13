using Amazon.CDK;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.SQS;
using BankMessages;
using Deploy;

class LoanBrokerStack : Stack
{

    public LoanBrokerStack(Construct scope, string id, IStackProps? props = null)
        : base(scope, id, props)
    {
        _ = new NServiceBusEndpointResource(new EndpointDetails("LoanBroker"){ EnableDynamoDBPersistence = true}, this, "LoanBroker.LoanBroker");
        _ = new NServiceBusEndpointResource(new EndpointDetails("Client"), this, "LoanBroker.Client");
        var endpointDetails = new EndpointDetails("Bank1Adapter");
        endpointDetails.EventsToSubscribe = [typeof(QuoteRequested)];
        _ = new NServiceBusEndpointResource(endpointDetails, this, "LoanBroker.Bank1Adapter");
        _ = new NServiceBusEndpointResource(new EndpointDetails("Bank2Adapter"), this, "LoanBroker.Bank2Adapter");
        _ = new NServiceBusEndpointResource(new EndpointDetails("Bank3Adapter"), this, "LoanBroker.Bank3Adapter");

        _ = new Queue(this, "error", new QueueProps
        {
            QueueName =  "error",
            RetentionPeriod = Duration.Seconds(900)
        });



        TaskDefinition taskDefinition = new TaskDefinition(this, "ContainersTask", new TaskDefinitionProps());
        Cluster cluster;

        // Add a container to the task definition
        var grafanaContainer = taskDefinition.AddContainer("Grafana", new ContainerDefinitionOptions {
            Image = ContainerImage.FromRegistry("grafana/grafana-oss:latest"),
            MemoryLimitMiB = 2048
        });

        grafanaContainer.AddPortMappings(new PortMapping {
            ContainerPort = 3000,
            Protocol = Protocol. TCP
        });

        var volume = new Volume()
        {
            Name = "Grafana",
            DockerVolumeConfiguration = new DockerVolumeConfiguration()
            {
                Autoprovision = true
            }
        };

        taskDefinition.AddVolume(volume);
        // grafanaContainer.AddVolumesFrom(new VolumeFrom
        // {
        //
        // });

        // grafana:
        // TODO: restart: unless-stopped ---> policy from AWS UI
        // volumes:
        // - ./grafana/provisioning:/etc/grafana/provisioning/
        //     - ./grafana/dashboards:/var/lib/grafana/dashboards
        //     - ./volumes/grafana-data:/var/lib/grafana
        // networks:
        // - ls

        // new Ec2Service(this, "Service", new Ec2ServiceProps {
        //     Cluster = cluster,
        //     TaskDefinition = taskDefinition,
        //     CloudMapOptions = new CloudMapOptions {
        //         // Create SRV records - useful for bridge networking
        //         DnsRecordType = DnsRecordType. SRV,
        //         // Targets port TCP port 7600 `specificContainer`
        //         Container = specificContainer,
        //         ContainerPort = 7600
        //     }
        // })
        // new ContainerDefinition()
    }

}