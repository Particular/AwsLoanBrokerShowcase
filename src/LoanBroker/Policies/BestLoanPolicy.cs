using LoanBroker.Services;
using Messages;
using Microsoft.Extensions.Logging;

namespace LoanBroker.Policies;

class BestLoanPolicy(ILogger<BestLoanPolicy> log, ICreditScoreProvider creditScoreProvider, IQuoteAggregator quoteAggregator) : Saga<BestLoanData>,
    IAmStartedByMessages<FindBestLoan>,
    IHandleMessages<QuoteCreated>,
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
        // Business logic here
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
        Data.Quotes.Add(new Quote(message.BankIdentifier, message.InterestRate));
        return Task.CompletedTask;
    }

    public async Task Timeout(MaxTimeout timeout, IMessageHandlerContext context)
    {
        var best = quoteAggregator.Reduce(Data.Quotes);
        await ReplyToOriginator(context, new BestLoanFound(Data.RequestId, best));
        MarkAsComplete();
    }
}

class BestLoanData : ContainSagaData
{
    // TODO do we need setters or the serializer will complain?
    public string RequestId { get; set; }
    public List<Quote> Quotes { get; set; } = [];
}

record MaxTimeout();

