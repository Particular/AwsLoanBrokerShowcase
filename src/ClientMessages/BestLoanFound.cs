using NServiceBus;

namespace Messages;

public record BestLoanFound(
    string RequestId,
    string BankId,
    double InterestRate) : IMessage;