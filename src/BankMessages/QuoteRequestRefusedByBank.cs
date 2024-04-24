using NServiceBus;

namespace Messages;

public record QuoteRequestRefusedByBank(
    string RequestId,
    string BankId) : IMessage;