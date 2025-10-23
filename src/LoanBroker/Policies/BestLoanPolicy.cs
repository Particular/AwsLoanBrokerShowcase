using System.Diagnostics;
using BankMessages;
using ClientMessages;
using LoanBroker.Messages;
using LoanBroker.Services;
using Microsoft.Extensions.Logging;
using CommonMessages;

namespace LoanBroker.Policies;

class BestLoanPolicy(
    ILogger<BestLoanPolicy> logger,
    IQuoteAggregator quoteAggregator) : Saga<BestLoanData>,
    IAmStartedByMessages<FindBestLoanWithScore>,
    IHandleMessages<QuoteCreated>,
    IHandleMessages<QuoteRequestRefusedByBank>,
    IHandleTimeouts<MaxTimeout>
{
    public async Task Handle(FindBestLoanWithScore message, IMessageHandlerContext context)
    {
        logger.LogInformation($"FindBestLoan request received from {message.Prospect}, with ID {message.RequestId}. Details: number of years {message.NumberOfYears}, amount: {message.Amount}");

        var publishOptions = new PublishOptions();
        publishOptions.ContinueExistingTraceOnReceive();
        await context.Publish(new QuoteRequested(message.RequestId,
            message.Score,
            message.NumberOfYears,
            message.Amount
        ), publishOptions);

        Data.RequestSentToBanks = DateTime.UtcNow;
        var requestExpiration = TimeSpan.FromSeconds(10);
        var timeoutMessage = new MaxTimeout { RequestId = message.RequestId };
        await RequestTimeout(context, requestExpiration, timeoutMessage);
        logger.LogInformation($"Quote, with request ID {message.RequestId}, requested to banks. The request expires in {requestExpiration}");
    }

    public Task Handle(QuoteCreated message, IMessageHandlerContext context)
    {
        logger.LogInformation($"Quote, for request ID {message.RequestId}, received from bank {message.BankId}. Interest rate: {message.InterestRate}");
        Data.Quotes.Add(new Quote(message.BankId, message.InterestRate));

        var tags = new TagList(
        [
            new(Tags.MessageType, typeof(QuoteCreated)),
            new(Tags.BankId,message.BankId),
        ]);

        if (Data.RequestSentToBanks.HasValue)
        {
            CustomTelemetry.BankResponseTime.Record((DateTime.UtcNow - Data.RequestSentToBanks!.Value).TotalMilliseconds, tags);
        }
        return Task.CompletedTask;
    }

    public Task Handle(QuoteRequestRefusedByBank message, IMessageHandlerContext context)
    {
        logger.LogWarning($"Quote, for request ID {message.RequestId}, refused by bank {message.BankId}. Request: {message.RequestId}");
        Data.RejectedBy.Add(message.BankId);

        var tags = new TagList(
        [
            new(Tags.MessageType, typeof(QuoteRequestRefusedByBank)),
            new(Tags.BankId,message.BankId),
        ]);

        if (Data.RequestSentToBanks.HasValue)
        {
            CustomTelemetry.BankResponseTime.Record((DateTime.UtcNow - Data.RequestSentToBanks!.Value).TotalMilliseconds, tags);
        }
        return Task.CompletedTask;
    }

    public async Task Timeout(MaxTimeout timeout, IMessageHandlerContext context)
    {
        IEvent eventToPublish;

        if (Data.Quotes.Count > 0)
        {
            var quote = quoteAggregator.Reduce(Data.Quotes);
            eventToPublish = new BestLoanFound(Data.RequestId, quote.BankId, quote.InterestRate);
            logger.LogInformation($"Best Loan found for request ID {Data.RequestId}, from bank {quote.BankId}. Details, interest rate: {quote.InterestRate}");
        }
        else if (Data.RejectedBy.Count > 0)
        {
            eventToPublish = new QuoteRequestRefused(Data.RequestId);
            logger.LogWarning($"All banks that responded rejected the quote request with ID {Data.RequestId}");
        }
        else
        {
            eventToPublish = new NoQuotesReceived(Data.RequestId);
            logger.LogWarning($"The request ID {Data.RequestId} expired with no responses from banks");
        }

        await context.Publish(eventToPublish);
        MarkAsComplete();
    }

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BestLoanData> mapper)
    {
        mapper.MapSaga(saga => saga.RequestId)
            .ToMessage<FindBestLoanWithScore>(message => message.RequestId)
            .ToMessage<QuoteCreated>(message => message.RequestId)
            .ToMessage<QuoteRequestRefusedByBank>(message => message.RequestId);
    }
}

class BestLoanData : ContainSagaData
{
    public DateTime? RequestSentToBanks { get; set; } = null;
    public string RequestId { get; set; } = null!;
    public List<Quote> Quotes { get; set; } = [];
    public List<string> RejectedBy { get; set; } = [];
}

public class MaxTimeout : ILoanMessage
{
    public string RequestId { get; set; }
}

static class Tags
{
    public const string MessageType = "nservicebus.message_type";
    public const string BankId = "loan_broker.bank_id";
}
