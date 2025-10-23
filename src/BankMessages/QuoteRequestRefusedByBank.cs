using NServiceBus;
using CommonMessages;

namespace BankMessages;

public record QuoteRequestRefusedByBank(
    string RequestId,
    string BankId) : IMessage, ILoanMessage;