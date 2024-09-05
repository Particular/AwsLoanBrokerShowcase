using Amazon.CDK;
using Amazon.CDK.AWS.SQS;
using BankMessages;
using Deploy;

var app = new App();

new LoanBrokerStack(app, "LoanBroker");
//new LambdaStack(app, "CreditBureau", new StackProps());

var cloudAssembly = app.Synth();


class LoanBrokerStack : Stack
{

    public LoanBrokerStack(Construct scope, string id, IStackProps? props = null)
        : base(scope, id, props)
    {
        new NServiceBusEndpointResource(new EndpointDetails("LoanBroker"), this, "LoanBroker.LoanBroker");
        new NServiceBusEndpointResource(new EndpointDetails("Client"), this, "LoanBroker.Client");
        var endpointDetails = new EndpointDetails("Bank1Adapter");
        endpointDetails.EventsToSubscribe = [typeof(QuoteRequested)];
        new NServiceBusEndpointResource(endpointDetails, this, "LoanBroker.Bank1Adapter");
        new NServiceBusEndpointResource(new EndpointDetails("Bank2Adapter"), this, "LoanBroker.Bank2Adapter");
        new NServiceBusEndpointResource(new EndpointDetails("Bank3Adapter"), this, "LoanBroker.Bank3Adapter");

        var error = new Queue(this, "error", new QueueProps
        {
            QueueName =  "error",
            RetentionPeriod = Duration.Seconds(900)
        });
    }

}