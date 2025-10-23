using BankMessages;
using ClientMessages;
using CommonConfigurations;
using LoanBroker.Messages;
using LoanBroker.Policies;
using Microsoft.Azure.Cosmos;

static class EndpointCustomizations
{
    public static void ApplyLoanBrokerEndpointCustomizations(this Customizations customizations)
    {
        customizations.Persistence.Sagas().UsePessimisticLocking();

        var transactionInformation = customizations.Persistence.TransactionInformation();

        // transactionInformation.ExtractContainerInformationFromMessage<FindBestLoanWithScore>(m =>
        // {
        //     //logger.LogInformation($"Message '{m.GetType().AssemblyQualifiedName}' destined to be handled by '{nameof(ShipOrderSaga)}' will use 'ShipOrderSagaData' container.");
        //     return new ContainerInformation("BestLoanData", new PartitionKeyPath("/id"));
        // });
        //
        // transactionInformation.ExtractContainerInformationFromHeaders(headers =>
        // {
        //     if (headers.TryGetValue(Headers.SagaType, out var sagaTypeHeader) &&
        //         sagaTypeHeader.Contains(nameof(BestLoanPolicy)))
        //     {
        //         //logger.LogInformation($"Message '{headers[Headers.EnclosedMessageTypes]}' destined to be handled by '{nameof(ShipOrderSaga)}' will use 'ShipOrderSagaData' container.");
        //
        //         return new ContainerInformation("BestLoanData", new PartitionKeyPath("/id"));
        //     }
        //
        //     return null;
        // });

        transactionInformation.ExtractPartitionKeyFromMessage<FindBestLoan>(message => new PartitionKey(message.RequestId));
        transactionInformation.ExtractPartitionKeyFromMessage<FindBestLoanWithScore>(message => new PartitionKey(message.RequestId));
        transactionInformation.ExtractPartitionKeyFromMessage<QuoteCreated>(message => new PartitionKey(message.RequestId));
        transactionInformation.ExtractPartitionKeyFromMessage<QuoteRequestRefusedByBank>(message => new PartitionKey(message.RequestId));
        transactionInformation.ExtractPartitionKeyFromMessage<MaxTimeout>(message => new PartitionKey(message.RequestId));

        customizations.Routing.RouteToEndpoint(typeof(FindBestLoanWithScore), "LoanBroker");
    }
}