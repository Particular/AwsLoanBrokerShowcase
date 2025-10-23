using NServiceBus;
using CommonMessages;

namespace ClientMessages;

public record BestLoanFound(
    string RequestId,
    string BankId,
    double InterestRate) : IEvent, ILoanMessage;