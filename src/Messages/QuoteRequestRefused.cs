namespace Messages;

public record QuoteRequestRefused(
    string RequestId,
    string BankId);