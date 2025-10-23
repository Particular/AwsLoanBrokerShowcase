using NServiceBus;
using CommonMessages;

namespace ClientMessages;

public record QuoteRequestRefused(string RequestId) : IEvent, ILoanMessage;