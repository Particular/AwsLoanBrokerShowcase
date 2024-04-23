using LoanBroker.Services;
using Messages;
using Microsoft.Extensions.Logging;

namespace LoanBroker.Policies;

class BestLoanPolicy(
    ILogger<BestLoanPolicy> log,
    ICreditScoreProvider creditScoreProvider,
    IQuoteAggregator quoteAggregator) : Saga<BestLoanData>,
    IAmStartedByMessages<FindBestLoan>,
    IHandleMessages<QuoteCreated>,
    IHandleMessages<QuoteRequestRefused>,
    IHandleTimeouts<MaxTimeout>
{
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BestLoanData> mapper)
    {
        mapper.MapSaga(saga => saga.RequestId)
            .ToMessage<FindBestLoan>(message => message.RequestId)
            .ToMessage<QuoteCreated>(message => message.RequestId);
    }

    public async Task Handle(FindBestLoan message, IMessageHandlerContext context)
    {
        var score = creditScoreProvider.Score();
        await context.Publish(new QuoteRequested(message.RequestId,
            message.Prospect,
            score,
            message.NumberOfYears,
            message.Amount
        ));
        await RequestTimeout<MaxTimeout>(context, TimeSpan.FromMinutes(10));
    }

    public Task Handle(QuoteCreated message, IMessageHandlerContext context)
    {
        Data.Quotes ??= [];
        Data.Quotes.Add(new Quote(message.BankIdentifier, message.InterestRate));
        return Task.CompletedTask;
    }

    public Task Handle(QuoteRequestRefused message, IMessageHandlerContext context)
    {
        Data.RejectedBy ??= [];
        Data.RejectedBy.Add(message.BankId);
        return Task.CompletedTask;
    }

    public async Task Timeout(MaxTimeout timeout, IMessageHandlerContext context)
    {
        // Add some logic about whether banks were not available vs. banks refused the request
        var best = quoteAggregator.Reduce(Data.Quotes ?? []);
        await ReplyToOriginator(context, new BestLoanFound(Data.RequestId, best));
        MarkAsComplete();
    }
}

class BestLoanData : ContainSagaData
{
    public string RequestId { get; set; } = null!;

    //DynamoDB does not accept empty list, they must be null.
    public List<Quote>? Quotes { get; set; }

    public List<string>? RejectedBy { get; set; }
}

record MaxTimeout();