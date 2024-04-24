using NServiceBus;

namespace Messages;

public record QuoteRequested(
    string RequestIdentifier,
    int Score,
    int NumberOfYears,
    int Amount
) : IEvent;