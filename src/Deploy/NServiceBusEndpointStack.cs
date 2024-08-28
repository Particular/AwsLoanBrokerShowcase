using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Amazon.CDK;
using Amazon.CDK.AWS.SQS;

namespace Deploy;

public class NServiceBusEndpointStack : Stack
{
    public NServiceBusEndpointStack(EndpointDetails endpoint, Construct scope, string id, IStackProps? props)
        : base(scope, id, props)
    {
        var queue = new Queue(scope, endpoint.EndpointName, new QueueProps
        {
            QueueName = endpoint.FullQueueName,
            RetentionPeriod = Duration.Seconds(endpoint.RetentionPeriod.TotalSeconds)
        });

        var delayed = new Queue(scope, $"{endpoint.EndpointName}.delays", new QueueProps
        {
            QueueName = endpoint.DelayQueueName,
            Fifo = true,
            RetentionPeriod = Duration.Seconds(endpoint.RetentionPeriod.TotalSeconds)
        });

        var error = new Queue(scope, "error", new QueueProps
        {
            QueueName = endpoint.FullQueueName,
            RetentionPeriod = Duration.Seconds(endpoint.RetentionPeriod.TotalSeconds)
        });
    }
}

public class EndpointDetails(string endpointName)
{
    public string EndpointName => endpointName;
    public string? Prefix { get; set; }
    public string ErrorQueue { get; set; } = "error";

    public string FullQueueName => $"{Prefix}{endpointName}";

    public string DelayQueueName => $"{Prefix}{endpointName}-delay.fifo";

    public string FullErrorQueueName => $"{Prefix}{ErrorQueue}";


    public TimeSpan RetentionPeriod { get; set; } = TimeSpan.FromDays(4);
}
