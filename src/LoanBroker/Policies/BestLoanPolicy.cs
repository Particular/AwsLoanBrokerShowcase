using BankMessages;
using ClientMessages;
using LoanBroker.Services;

namespace LoanBroker.Policies;

class BestLoanPolicy(
    ICreditScoreProvider creditScoreProvider,
    IQuoteAggregator quoteAggregator) : Saga<BestLoanData>,
    IAmStartedByMessages<FindBestLoan>,
    IHandleMessages<QuoteCreated>,
    IHandleMessages<QuoteRequestRefusedByBank>,
    IHandleTimeouts<MaxTimeout>
{
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BestLoanData> mapper)
    {

        mapper.MapSaga(saga => saga.RequestId)
            .ToMessage<FindBestLoan>(message => message.RequestId)
            .ToMessage<QuoteCreated>(message => message.RequestId)
            .ToMessage<QuoteRequestRefusedByBank>(message => message.RequestId);
    }

    public async Task Handle(FindBestLoan message, IMessageHandlerContext context)
    {
        var score = creditScoreProvider.Score(message.Prospect);
        await context.Publish(new QuoteRequested(message.RequestId,
            score,
            message.NumberOfYears,
            message.Amount
        ));
        await RequestTimeout<MaxTimeout>(context, TimeSpan.FromSeconds(10));
    }

    public Task Handle(QuoteCreated message, IMessageHandlerContext context)
    {
        Data.Quotes ??= [];
        Data.Quotes.Add(new Quote(message.BankIdentifier, message.InterestRate));
        return Task.CompletedTask;
    }

    public Task Handle(QuoteRequestRefusedByBank message, IMessageHandlerContext context)
    {
        Data.RejectedBy ??= [];
        Data.RejectedBy.Add(message.BankId);
        return Task.CompletedTask;
    }

    public async Task Timeout(MaxTimeout timeout, IMessageHandlerContext context)
    {
        var receivedQuotes = Data.Quotes ?? [];
        var receivedRejections = Data.RejectedBy ?? [];

        IMessage replyMessage;

        if (receivedQuotes.Count > 0)
        {
            var quote = quoteAggregator.Reduce(receivedQuotes);
            replyMessage = new BestLoanFound(Data.RequestId, quote.BankId, quote.InterestRate);
        }
        else if (receivedRejections.Count > 0)
        {
            replyMessage = new QuoteRequestRefused(Data.RequestId);
        }
        else
        {
            replyMessage = new NoQuotesReceived(Data.RequestId);
        }

        await ReplyToOriginator(context, replyMessage);
        MarkAsComplete();
    }
}

class BestLoanData : ContainSagaData
{
    public string RequestId { get; set; } = null!;
    public List<Quote>? Quotes { get; set; }
    public List<string>? RejectedBy { get; set; }
}

record MaxTimeout;