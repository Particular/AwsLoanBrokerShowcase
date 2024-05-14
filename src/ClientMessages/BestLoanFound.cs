using NServiceBus;

namespace ClientMessages;

public record BestLoanFound(
    string RequestId,
    string BankId,
    double InterestRate) : IMessage;