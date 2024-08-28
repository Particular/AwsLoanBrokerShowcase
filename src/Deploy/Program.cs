using Amazon.CDK;
using Deploy;

var app = new App();

new LambdaStack(app, "CreditBureau", new StackProps());

var cloudAssembly = app.Synth();