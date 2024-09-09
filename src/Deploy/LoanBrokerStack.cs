using Amazon.CDK;
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
    }

}