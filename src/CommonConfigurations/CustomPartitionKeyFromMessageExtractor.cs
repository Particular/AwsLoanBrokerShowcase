using CommonMessages;
using Microsoft.Azure.Cosmos;
using NServiceBus.Persistence.CosmosDB;

namespace CommonConfigurations;

class CustomPartitionKeyFromMessageExtractor : IPartitionKeyFromMessageExtractor
{
    public bool TryExtract(object message, IReadOnlyDictionary<string, string> headers, out PartitionKey? partitionKey)
    {
        if (message is ILoanMessage loanMessage)
        {
            partitionKey = new PartitionKey(loanMessage.RequestId);
            return true;
        }

        partitionKey = null;
        return false;
    }
}