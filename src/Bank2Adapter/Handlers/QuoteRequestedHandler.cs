using BankMessages;
using Microsoft.Extensions.Logging;

namespace Bank2Adapter.Handlers;

public class QuoteRequestedHandler(ILogger<QuoteRequestedHandler> logger) : IHandleMessages<QuoteRequested>
{
    static readonly Random Random = new();
    const string BankIdentifier = "Bank2";

    public async Task Handle(QuoteRequested message, IMessageHandlerContext context)
    {
        // Simulate bank latency
        await Task.Delay(Random.Shared.Next(0, 5000), context.CancellationToken);

        logger.LogInformation($"Quote request with ID {message.RequestId}. Details: number of years {message.NumberOfYears}, amount: {message.Amount}, credit score: {message.Score}");

        if (DateTime.Now.Ticks % 300 == 0)
        {
            throw new Exception("Random error");
        }

        if (DateTime.Now.Ticks % 5 == 0)
        {
            // Simulate additional bank latency
            await Task.Delay(Random.Shared.Next(1000, 5000), context.CancellationToken);
        }

        if (Random.Next(0, 5) == 0 || message.Score < 90)
        {
            var quoteRejected = new QuoteRequestRefusedByBank(message.RequestId, BankIdentifier);
            logger.LogWarning($"Quote for request ID {message.RequestId} refused.");

            await context.Reply(quoteRejected);
        }
        else
        {
            var interestRate = Random.NextDouble();
            var quoteCreated = new QuoteCreated(message.RequestId, BankIdentifier, interestRate);
            logger.LogInformation($"Quote created for request ID {message.RequestId}. Details: interest rate: {interestRate}");

            await context.Reply(quoteCreated);
        }
    }
}
