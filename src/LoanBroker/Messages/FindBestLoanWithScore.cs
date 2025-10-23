using ClientMessages;
using CommonMessages;

namespace LoanBroker.Messages;

public record FindBestLoanWithScore(
    string RequestId,
    Prospect Prospect,
    int NumberOfYears,
    int Amount,
    int Score
) : ICommand, ILoanMessage;