# AWS LoanBroker example

The AWS LoanBroker example is a basic loan broker implementation following the [structure presented](https://www.enterpriseintegrationpatterns.com/patterns/messaging/ComposedMessagingExample.html) by [Gregor Hohpe](https://www.enterpriseintegrationpatterns.com/gregor.html) in his [Enterprise Integration Pattern](https://www.enterpriseintegrationpatterns.com/) book.

The example is composed by:

- A client application, sending loan requests.
- A credit bureau providing the customers' credit score.
- A loan broker service that receives loan requests, enriches them with credit scores, and orchestrates communication with downstream banks.
- Three bank adapters, acting like Anti Corruption layers (ACL), simulating communication with downstream banks offering loans.
- An email sender simulating email communication with customers.

The example also ships the following monitoring services:

- The Particular platform to monitor endpoints, capture and visualize audit messages, manage failed messages.
- A Prometheus instance to collect, store, and query raw metrics data.
- A Grafana instance with three different metrics dashboards, using Prometheus as data source.
- A Jaeger instance to visualize OpenTelemetry traces.
- AWS Distro for OpenTelemetry collector (ADOT), to collect and export metrics and traces to various destinations.

The example also exports metrics and traces to AWS CloudWatch and XRay.

## Requirements

- .NET 8 or greater
- Docker
- Docker Compose

## How to run the example

The simplest way to run the example is using Docker for both the endpoints and the infrastructure.
The client application, the loan broker service, the e-mail sender and the bank adapters can be deployed as Docker containers alongside the Particular platform to monitor the system, LocalStack to mock the AWS services, and the additional containers needed for enabling OpenTelemetry observability. 

To run the full example in Docker, execute the following command from the `src` folder:

```shell
docker compose up --build -d
```

The above command will build all projects, build container images, deploy them to the local Docker registry, and start them. 
The Docker Compose command will also run and configure all the additional infrastructural containers.

To stop the running solution and remove all deployed containers. Using a command prompt, from the `src` folder, execute the following command:

```shell
docker compose down
```

To run the solution without rebuilding container images from the `src` folder, using a command prompt, execute the following command:

```shell
docker compose up -d
```

> [!Note]
> To run transport and persistence using AWS services instead of LocalStack: 
> - remove the `AWS_ENDPOINT_URL` variable from the [aws.env](src/aws.env) file assure
> - ensure the following environment variables are defined with appropriate values:
>   - `AWS_ACCESS_KEY_ID`
>   - `AWS_SECRET_ACCESS_KEY`
>   - `AWS_REGION`

### Running endpoints from the IDE

If you prefer to start the endpoints from your IDE in order to debug the code, from the `src` folder, using a command prompt, execute the following command to start the required infrastructure:

```shell
docker compose --profile infrastructure up -d
```

## Monitoring

The example comes with the [Particular platform](https://docs.particular.net/platform/) automatically available as
Docker containers.

Monitoring information are available in [ServicePulse](http://localhost:9999).

[ServiceInsight](https://docs.particular.net/serviceinsight/) can be used in Windows environments to visualize messages
flow. You can download the latest version of ServiceInsight from
the [Particular website](https://particular.net/serviceinsight).

## Telemetry

NServiceBus supports OpenTelemetry. Starting with NServiceBus 9.1, the following metrics are available:

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

All endpoints are configured to send OpenTelemetry traces to Jaeger. To visualize traces, open the [Jaeger dashboard](http://localhost:16686).

Similarly, endpoints send metrics to Prometheus. To visualize metrics, open the [Grafana dashboards](http://localhost:3000/dashboards). The default Grafana credentials are:

- Username: `admin`
- Passowrd: `admin`

> [!NOTE]
> Setting a new password can be skipped. When containers are redeployed, the credentials are reset to their default values.

The example deploys two pre-configured Grafana dashboards:

- The [LoanBroker](http://localhost:3000/d/edmhjobnxatc0b/loanbroker?orgId=1&refresh=5s) dashboard shows various metrics about the business endpoints behavior, such as the differences between the services critical, processing, and handing time.
- The [NServiceBus](http://localhost:3000/d/MHqYOIqnz/nservicebus?orgId=1&refresh=5s) dashboard shows the metrics, grouped by endpoints or message type related to message fetches, processing, and failures.  

> [!NOTE]
> After running the solution multiple times, it might happen that Grafana suddenly shows random data instead of the expected metrics. To reset dashboards, tear down all containers and delete the `data-grafana` and `data-prometheus` folders from the solution folder. Redeploy the containers.
