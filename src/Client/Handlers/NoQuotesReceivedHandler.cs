using ClientMessages;
using Microsoft.Extensions.Logging;

namespace Client.Handlers;

[Handler]
public class NoQuotesReceivedHandler(ILogger<NoQuotesReceivedHandler> logger) : IHandleMessages<NoQuotesReceived>
{
    public Task Handle(NoQuotesReceived message, IMessageHandlerContext context)
    {
        logger.LogInformation("No banks available to process the loan request with id {RequestId}, try again later", message.RequestId);
        return Task.CompletedTask;
    }
}