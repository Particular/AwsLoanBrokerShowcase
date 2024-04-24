using Messages;
using Microsoft.Extensions.Logging;

namespace Client.Handlers;

public class QuoteRequestRefusedHandler(ILogger<QuoteRequestRefusedHandler> logger) : IHandleMessages<QuoteRequestRefused>
{
    public Task Handle(QuoteRequestRefused message, IMessageHandlerContext context)
    {
        logger.LogInformation("The request id {RequestId} has been refused by all available banks", message.RequestId);
        return Task.CompletedTask;
    }
}