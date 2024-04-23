using Messages;
using Microsoft.Extensions.Logging;

namespace Client.Handlers;

public class QuoteRequestRejectedHandler(ILogger<QuoteRequestRejectedHandler> logger) : IHandleMessages<QuoteRequestRejected>
{
    public Task Handle(QuoteRequestRejected message, IMessageHandlerContext context)
    {
        logger.LogInformation("The request id {RequestId} has been rejected by all available banks", message.RequestId);
        return Task.CompletedTask;
    }
}