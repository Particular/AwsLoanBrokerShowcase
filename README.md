# AWS LoanBroker Sample

The sample is a basic LoanBroker implementation following the [structure presented](https://www.enterpriseintegrationpatterns.com/patterns/messaging/ComposedMessagingExample.html) by [Gregor Hohpe](https://www.enterpriseintegrationpatterns.com/gregor.html) in his [Enterprise Integration Pattern](https://www.enterpriseintegrationpatterns.com/) book.

The sample is composed by:

- A client application, sending loan requests.
- A loan broker service that receives loan requests and orchestrates communication with downstream banks.
- Three bank adapters, acting like Anti Corruption layers (ACL), simulating communication with downstream banks offering loans.

The sample also ships the following monitoring services:

- A Grafana instance with two different metrics dashboards
- A Prometheus instance to query raw metrics data
- A Jaeger instance to visualize OpenTelemetry traces

## Requirements

- .NET 8 or greater
- Docker
- Docker Compose

## How to run the sample from the development IDE

To mock AWS services, the sample uses [LocalStack](https://www.localstack.cloud/) in a Docker container. Using a command prompt, run LocalStack first by issuing the following command in the `src` directory:

```shell
docker-compose up localstack, prometheus, grafana, jaeger, adot
```

The above command will execute the sample `docker-compose.yml` file, starting only with the necessary infrastructural components, such as the LocalStack container and the containers required for monitoring the endpoints.

Once the LocalStack container is up and running, from the development environment, start the following projects:

- Client
- LoanBroker
- BankAdapter1
- BankAdapter2
- BankAdapter3

To stop the LocalStack and infrastructure containers, at the command prompt, issue the following command from the `src` folder:

```shell
docker-compose stop localstack, prometheus, grafana, jaeger, adot
```

If you are not interested in metrics and traces, it is possible to start only the `localstack` container, excluding the following containers: `prometheus`, `grafana`, `jaeger`, and `adot`. Without them, metrics and traces will not be captured. 

## How to run the sample using Docker containers

The client application, the LoanBroker service, and the bank adapters can be deployed as Docker containers alongside the LocalStack one to mock the AWS services. To do so, from the `src` folder, execute the following command:  

```shell
docker-compose up --build
```

The above command will build all projects, build container images, deploy them to the local Docker registry, and start them. The Docker Compose command will also run and configure all the containers needed to capture and visualize OpenTelemetry traces and metrics.

To run the solution without rebuilding container images from the `src` folder, using a command prompt, execute the following command:

```shell
docker-compose up
```

The docker-compose configuration will start the following containers:

- LocalStack
- Client
- LoanBroker
- BankAdapter1
- BankAdapter2
- BankAdapter3

Alongside the containers required to capture and visualize metrics and traces:

- adot
- Prometheus
- Grafana
- Jaeger

All containers will use the same network as the LocalStack container instance.

To interact with the sample, attach a console to the Client running container by executing the following command:

```shell
docker attach loanbroker-client-1
```

Once attached, use the `F` key to send one loan request. Use the `L` key to send a loan request every second. Sending one request every second is useful for simulating some load and then visualizing rich metrics and traces in Grafana and Jaeger.

To detach from an attached container, use `Ctrl+P + Ctrl+Q`.

To stop the sample, take down all running containers from the `src` folder, using a command prompt, execute the following command:

```shell
docker-compose down
```

### Telemetry

NServiceBus supports OpenTelemetry. Starting with NServiceBus 9.1, the following metrics can be emitted:

- `nservicebus.messaging.successes` - Total number of messages processed successfully by the endpoint
- `nservicebus.messaging.fetches` - Total number of messages fetched from the queue by the endpoint
- `nservicebus.messaging.failures` - Total number of messages processed unsuccessfully by the endpoint
- `nservicebus.messaging.handler_time` - The time the user handling code takes to handle a message
- `nservicebus.messaging.processing_time` - The time the endpoint takes to process a message
- `nservicebus.messaging.critical_time` - The time between when a message is sent and when it is fully processed
- `nservicebus.recoverability.immediate` - Total number of immediate retries requested
- `nservicebus.recoverability.delayed` - Total number of delayed retries requested
- `nservicebus.recoverability.error` - Total number of messages sent to the error queue

For more information, refer to the [NServiceBus OpenTelemetry documentation](https://docs.particular.net/nservicebus/operations/opentelemetry).

All sample endpoints are configured to send OpenTelemetry traces to Jaeger. To visualize traces, open the [Jaeger dashboard](http://localhost:16686).

Similarly, endpoints send metrics to Prometheus. To visualize metrics, open the [Prometheus dashboards](http://localhost:3000/dashboards). There are two pre-configured dashboards:

- TBD

### Sample scenarios

TODO

- Press F on the client and observe messages flowing bla bla
- Stop all bank adapters, press F on the client and observe the behavior
- Stop the LoanBroker, press F on the client and stop the client, start the LoanBroker observe messages flowing, start the client and observe the Loanbroker response eventually coming in.

## How to modify the same to run against an AWS Account

TODO
=======
# AwsLoanBrokerSample

## LocalStack setup

Docker compose file:

```
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
