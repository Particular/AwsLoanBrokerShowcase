using NServiceBus;

namespace ClientMessages;

public record FindBestLoan(
    string RequestId,
    Prospect Prospect,
    int NumberOfYears,
    int Amount
) : ICommand;