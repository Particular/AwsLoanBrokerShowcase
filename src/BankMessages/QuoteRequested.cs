using NServiceBus;
using CommonMessages;

namespace BankMessages;

public record QuoteRequested(
    string RequestId,
    int Score,
    int NumberOfYears,
    int Amount
) : IEvent, ILoanMessage;