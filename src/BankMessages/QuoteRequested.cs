using NServiceBus;

namespace BankMessages;

public record QuoteRequested(
    string RequestIdentifier,
    int Score,
    int NumberOfYears,
    int Amount
) : IEvent;