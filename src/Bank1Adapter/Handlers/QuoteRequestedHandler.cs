using BankMessages;

namespace Bank1Adapter.Handlers;

public class QuoteRequestedHandler : IHandleMessages<QuoteRequested>
{
    private const string BankIdentifier = "Bank1";

    public async Task Handle(QuoteRequested message, IMessageHandlerContext context)
    {
        if (message.Score < 900)
        {
            var quoteRejected = new QuoteRequestRefusedByBank(message.RequestIdentifier, BankIdentifier);

            await context.Reply(quoteRejected);
        }
        else
        {
            var interestRate = 0.1;
            var quoteCreated = new QuoteCreated(message.RequestIdentifier, BankIdentifier, interestRate);

            await context.Reply(quoteCreated);
        }
    }
}