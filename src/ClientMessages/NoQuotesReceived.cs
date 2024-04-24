using NServiceBus;

namespace Messages;

public record NoQuotesReceived(string RequestId) : IMessage;