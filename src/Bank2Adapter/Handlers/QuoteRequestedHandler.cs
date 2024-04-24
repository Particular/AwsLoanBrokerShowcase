using BankMessages;

namespace Bank2Adapter.Handlers;

public class QuoteRequestedHandler : IHandleMessages<QuoteRequested>
{
    private static readonly Random Random = new Random();
    private static readonly string BankIdentifier = "Bank2";

    public async Task Handle(QuoteRequested message, IMessageHandlerContext context)
    {
        if (Random.Next(0, 5) == 0 || message.Score < 90)
        {
            var quoteRejected = new QuoteRequestRefusedByBank(message.RequestIdentifier, BankIdentifier);

            await context.Reply(quoteRejected);
        }
        else
        {
            var interestRate = Random.NextDouble();
            var quoteCreated = new QuoteCreated(message.RequestIdentifier, BankIdentifier, interestRate);

            await context.Reply(quoteCreated);
        }
    }
}