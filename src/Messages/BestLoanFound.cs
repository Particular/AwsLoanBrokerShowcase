using NServiceBus;

namespace Messages;

public record BestLoanFound(
    string RequestId,
    Quote Quote) : IMessage;