using Amazon.CDK;
using Amazon.CDK.AWS.ECS;

namespace Deploy;

internal class NServiceBusEndpointContainerStack : NServiceBusEndpointStack
{
    public NServiceBusEndpointContainerStack(EndpointDetails endpoint, ContainerDefinitionProps containerProps, Construct scope, string id, IStackProps? props)
        : base(endpoint, scope, id, props)
    {
        // Queues created by base class

        var container = new ContainerDefinition(scope, "container", containerProps);
    }
}