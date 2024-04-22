namespace Messages;

public record QuoteRequested(
    string RequestIdentifier,
    Prospect Prospect,
    int Score,
    int NumberOfYears,
    int Amount
);