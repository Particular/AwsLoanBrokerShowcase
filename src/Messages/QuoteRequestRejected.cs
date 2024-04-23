using NServiceBus;

namespace Messages;

public record QuoteRequestRejected(string RequestId) : IMessage;