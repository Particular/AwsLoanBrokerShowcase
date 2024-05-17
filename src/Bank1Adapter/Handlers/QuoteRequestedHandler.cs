using BankMessages;
using Microsoft.Extensions.Logging;

namespace Bank1Adapter.Handlers;

public class QuoteRequestedHandler(ILogger<QuoteRequestedHandler> logger) : IHandleMessages<QuoteRequested>
{
    const string BankIdentifier = "Bank1";

    public async Task Handle(QuoteRequested message, IMessageHandlerContext context)
    {
        logger.LogInformation($"Quote request with ID {message.RequestId}. Details: number of years {message.NumberOfYears}, amount: {message.Amount}, credit score: {message.Score}");
        if (message is { Score: < 900, Amount: < 1_000_000 })
        {
            var quoteRejected = new QuoteRequestRefusedByBank(message.RequestId, BankIdentifier);
            logger.LogWarning($"Quote for request ID {message.RequestId} refused.");

            await context.Reply(quoteRejected);
        }
        else
        {
            const double interestRate = 0.1;
            var quoteCreated = new QuoteCreated(message.RequestId, BankIdentifier, interestRate);
            logger.LogInformation($"Quote created for request ID {message.RequestId}. Details: interest rate: {interestRate}");

            await context.Reply(quoteCreated);
        }
    }
}