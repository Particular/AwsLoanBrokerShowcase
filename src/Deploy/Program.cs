using Amazon.CDK;

var app = new App();

new LoanBrokerStack(app, "LoanBroker");
//new LambdaStack(app, "CreditBureau", new StackProps());

var cloudAssembly = app.Synth();