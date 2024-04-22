namespace Messages;

public record FindBestLoan(
    string RequestId,
    Prospect Prospect,
    int NumberOfYears,
    int Amount
);