using NServiceBus;
using CommonMessages;

namespace ClientMessages;

public record NoQuotesReceived(string RequestId) : IEvent, ILoanMessage;