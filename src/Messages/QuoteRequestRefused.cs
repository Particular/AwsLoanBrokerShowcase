using NServiceBus;

namespace Messages;

public record QuoteRequestRefused(string RequestId) : IMessage;