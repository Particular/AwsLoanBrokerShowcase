using NServiceBus;
using CommonMessages;

namespace ClientMessages;

public record FindBestLoan(
    string RequestId,
    Prospect Prospect,
    int NumberOfYears,
    int Amount
) : ICommand, ILoanMessage;