using NServiceBus;
using CommonMessages;

namespace BankMessages;

public record QuoteCreated(
    string RequestId,
    string BankId,
    double InterestRate
) : IMessage, ILoanMessage;