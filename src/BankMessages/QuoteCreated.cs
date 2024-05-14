using NServiceBus;

namespace BankMessages;

public record QuoteCreated(
    string RequestId,
    string BankIdentifier,
    double InterestRate
) : IMessage;