using BankMessages;

namespace Bank1Adapter.Handlers;

public class QuoteRequestedHandler : IHandleMessages<QuoteRequested>
{
    private const string BankIdentifier = "Bank1";

    public async Task Handle(QuoteRequested message, IMessageHandlerContext context)
    {
        if (message is { Score: < 900, Amount: < 1_000_000 })
        {
            var quoteRejected = new QuoteRequestRefusedByBank(message.RequestIdentifier, BankIdentifier);

            await context.Reply(quoteRejected);
        }
        else
        {
            const double interestRate = 0.1;
            var quoteCreated = new QuoteCreated(message.RequestIdentifier, BankIdentifier, interestRate);

            await context.Reply(quoteCreated);
        }
    }
}