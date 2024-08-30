using Amazon.CDK;
using Deploy;

var app = new App();

new NServiceBusEndpointStack(new EndpointDetails("AWSLoanBrokerEndpoint"), app, "NServiceBusEndpoint", null);
//new LambdaStack(app, "CreditBureau", new StackProps());

var cloudAssembly = app.Synth();