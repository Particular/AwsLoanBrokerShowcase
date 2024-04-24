using NServiceBus;

namespace ClientMessages;

public record QuoteRequestRefused(string RequestId) : IMessage;