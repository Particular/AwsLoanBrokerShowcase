using NServiceBus;

namespace BankMessages;

public record QuoteRequested(
    string RequestId,
    int Score,
    int NumberOfYears,
    int Amount
) : IEvent;