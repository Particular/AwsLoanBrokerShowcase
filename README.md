# AwsLoanBrokerSample

## LocalStack setup

Docker compose file:

```
version: "3.3"
services:
  localstack:
    image: localstack/localstack
    environment:
      - SERVICES=sns,sqs,iam,s3,dynamodb
      - DEBUG=1
      - HOSTNAME=localstack
      - EDGE_PORT=4566
    ports:
      - '4566-4597:4566-4597'
      - "8000:5000"
```

Set the `ServiceUrl` of the various Amazon client config classes to `http://localhost:{EDGE_PORT}/`.
Set the various clients to use dummy `BasicAWSCredentials` :

```
var dummy = new BasicAWSCredentials("xxx","xxx");`
```

For example:

```
var edgeUrl = "http://localhost:4566";
var dummy = new BasicAWSCredentials("xxx","xxx");
var sqsConfig = new AmazonSQSConfig() { ServiceURL = edgeUrl };
var snsConfig = new AmazonSimpleNotificationServiceConfig(){ ServiceURL = edgeUrl };

var transport = new SqsTransport(
    new AmazonSQSClient(dummy, sqsConfig),
    new AmazonSimpleNotificationServiceClient(dummy, snsConfig));
```
The dummy credentials prevent the AWS clients from trying to pickup credentials from environment variables or connect to the AWS cloud IAM service to retrieve authorizations.
