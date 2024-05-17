using NServiceBus;

namespace BankMessages;

public record QuoteCreated(
    string RequestId,
    string BankId,
    double InterestRate
) : IMessage;