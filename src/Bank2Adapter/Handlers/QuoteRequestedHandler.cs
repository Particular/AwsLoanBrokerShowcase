using Messages;

namespace Bank2Adapter.Handlers;

public class QuoteRequestedHandler : IHandleMessages<QuoteRequested>
{
    private static readonly Random Random = new Random();
    private static readonly string BankIdentifier = "Bank2";

    public Task Handle(QuoteRequested message, IMessageHandlerContext context)
    {
        if (Random.Next(0, 5) == 0 || message.Score < 90)
        {
            // Reject request
        }
        else
        {
            var interestRate = Random.NextDouble();
            var quoteCreated = new QuoteCreated(message.RequestIdentifier, BankIdentifier, interestRate);

            //context.Reply()
        }

        return Task.CompletedTask;
    }
}