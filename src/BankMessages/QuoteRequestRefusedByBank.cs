using NServiceBus;

namespace BankMessages;

public record QuoteRequestRefusedByBank(
    string RequestId,
    string BankId) : IMessage;