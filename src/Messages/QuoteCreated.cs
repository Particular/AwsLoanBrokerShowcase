namespace Messages;

public record QuoteCreated(
    string RequestId,
    string BankIdentifier,
    double InterestRate
);