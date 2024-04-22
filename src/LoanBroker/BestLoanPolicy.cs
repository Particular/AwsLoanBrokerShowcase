using Microsoft.Extensions.Logging;

namespace Handlers;

using Messages;

internal class BestLoanPolicy(ILogger<BestLoanPolicy> log) : Saga<BestLoanData>,
    IAmStartedByMessages<FindBestLoan>,
    IHandleMessages<QuoteCreated>,
    IHandleTimeouts<MaxTimeout>
{
    private ICreditScoreProvider _creditScoreProvider;

    private IQuoteAggregator _quoteAggregator;

    
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BestLoanData> mapper)
    {
        // https://docs.particular.net/nservicebus/sagas/message-correlation
        mapper.MapSaga(saga => saga.RequestId)
            .ToMessage<FindBestLoan>(message => message.RequestId)
            .ToMessage<QuoteCreated>(message => message.RequestId);
    }

    public async Task Handle(FindBestLoan message, IMessageHandlerContext context)
    {
        // Business logic here
        var score = _creditScoreProvider.Score();
        await context.Publish(new QuoteRequested(message.RequestId,
            message.Prospect,
            score,
            message.NumberOfYears,
            message.Amount
        ));

        await RequestTimeout<MaxTimeout>(context, TimeSpan.FromMinutes(10));
    }

    public async Task Handle(QuoteCreated message, IMessageHandlerContext context)
    {
        Data.Quotes[message.BankIdentifier] = message.InterestRate;
        
    }


    public async Task Timeout(MaxTimeout timeout, IMessageHandlerContext context)
    {
        Quote best = _quoteAggregator.Reduce(Data.Quotes);
        await ReplyToOriginator(context, new BestLoanFound(Data.RequestId, best));
        MarkAsComplete();
    }
}

internal class BestLoanData : ContainSagaData
{
    public string RequestId { get; set; }

    public Dictionary<string, double> Quotes = new Dictionary<string, double>();
    // Other properties
}


internal record MaxTimeout();

