using NServiceBus;

namespace ClientMessages;

public record NoQuotesReceived(string RequestId) : IEvent;