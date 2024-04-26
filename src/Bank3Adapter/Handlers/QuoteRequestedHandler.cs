using BankMessages;

namespace Bank3Adapter.Handlers;

public class QuoteRequestedHandler : IHandleMessages<QuoteRequested>
{
    private static readonly Random Random = new();
    private const string BankIdentifier = "Bank3";

    public async Task Handle(QuoteRequested message, IMessageHandlerContext context)
    {
        if (Random.Next(0, 2) == 0)
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