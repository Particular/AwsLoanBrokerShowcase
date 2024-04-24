using LoanBroker.Policies;
using LoanBroker.Services;
using Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NServiceBus.Testing;
using NUnit.Framework;

namespace Tests;

public class BestLoanPolicyScenarioTests
{
    [Test]
    public async Task HappyPath()
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        var prospect = new Prospect("Scrooge", "McDuck");

        var initialCommand = new FindBestLoan(requestId, prospect, 30, 1_000_000);

        var policy = new TestableSaga<BestLoanPolicy, BestLoanData>(
            sagaFactory: () => new BestLoanPolicy(log, new FixedCreditScorer(800), new BestRateQuoteAggregator()));

        var result1 = await policy.Handle(initialCommand);
        var quoteRequested = result1.FindPublishedMessage<QuoteRequested>();
        var timeoutRequested = result1.FindTimeoutMessage<MaxTimeout>();

        Assert.That(quoteRequested, Is.Not.Null);
        Assert.That(quoteRequested.RequestIdentifier, Is.EqualTo(requestId));
        Assert.That(quoteRequested.NumberOfYears, Is.EqualTo(30));
        Assert.That(quoteRequested.Score, Is.EqualTo(800));

        (string BankId, double InterestRate)[] bankResponses =
        [
            new("FirstNational", 3.05),
            new("SecondRegional", 2.95),
            new("AreYouKidding", 99.99)
        ];

        var quoteCount = 0;
        foreach (var response in bankResponses)
        {
            var quoteResult = await policy.Handle(new QuoteCreated(requestId, response.BankId, response.InterestRate));
            Assert.That(quoteResult.Completed, Is.False);
            Assert.That(quoteResult.Context.SentMessages, Is.Empty);
            Assert.That(quoteResult.Context.PublishedMessages, Is.Empty);
            Assert.That(quoteResult.Context.TimeoutMessages, Is.Empty);
            Assert.That(quoteResult.SagaDataSnapshot.Quotes.Count, Is.EqualTo(++quoteCount));
        }

        var timeoutResults = await policy.AdvanceTime(TimeSpan.FromMinutes(10));

        Assert.That(timeoutResults.Length, Is.EqualTo(1));
        var onlyResult = timeoutResults.First();
        var responseMessage = onlyResult.FindReplyMessage<BestLoanFound>();

        Assert.That(responseMessage, Is.Not.Null);
        Assert.That(responseMessage.RequestId, Is.EqualTo(requestId));
        Assert.That(responseMessage.BankId, Is.EqualTo("SecondRegional"));
        Assert.That(responseMessage.InterestRate, Is.EqualTo(2.95));
    }

    [Test]
    public async Task NoResponsesFromBanks()
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        var prospect = new Prospect("Scrooge", "McDuck");
        var initialCommand = new FindBestLoan(requestId, prospect, 30, 1_000_000);

        var policy = new TestableSaga<BestLoanPolicy, BestLoanData>(
            sagaFactory: () => new BestLoanPolicy(log, new FixedCreditScorer(800), new BestRateQuoteAggregator()));

        await policy.Handle(initialCommand);
        var advanceTime = await policy.AdvanceTime(TimeSpan.FromMinutes(11));
        Assert.That(advanceTime.Length, Is.EqualTo(1));
        var handleResult = advanceTime[0];
        var replyMessage = handleResult.FindReplyMessage<NoQuotesReceived>();
        Assert.That(replyMessage.RequestId, Is.EqualTo(requestId));
        Assert.That(handleResult.Completed, Is.True);
        Assert.That(handleResult.Context.RepliedMessages.Length, Is.EqualTo(1));
        Assert.That(handleResult.Context.PublishedMessages, Is.Empty);
        Assert.That(handleResult.Context.TimeoutMessages, Is.Empty);
        Assert.That(handleResult.Context.SentMessages, Is.Empty);
    }

    [Test]
    public async Task LoanRequestRefusedByAllBanks()
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        var prospect = new Prospect("Scrooge", "McDuck");
        var initialCommand = new FindBestLoan(requestId, prospect, 30, 1_000_000);

        var policy = new TestableSaga<BestLoanPolicy, BestLoanData>(
            sagaFactory: () => new BestLoanPolicy(log, new FixedCreditScorer(800), new BestRateQuoteAggregator()));

        await policy.Handle(initialCommand);
        await policy.Handle(new QuoteRequestRefusedByBank(requestId, "bank1"));
        var advanceTime = await policy.AdvanceTime(TimeSpan.FromMinutes(11));
        Assert.That(advanceTime.Length, Is.EqualTo(1));
        var handleResult = advanceTime[0];
        var replyMessage = handleResult.FindReplyMessage<QuoteRequestRefused>();
        Assert.That(replyMessage.RequestId, Is.EqualTo(requestId));
        Assert.That(handleResult.Completed, Is.True);
        Assert.That(handleResult.Context.RepliedMessages.Length, Is.EqualTo(1));
        Assert.That(handleResult.Context.PublishedMessages, Is.Empty);
        Assert.That(handleResult.Context.TimeoutMessages, Is.Empty);
        Assert.That(handleResult.Context.SentMessages, Is.Empty);
    }

    [Test]
    public async Task LoanRequestRefusedByOneBankAcceptedByAnother()
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        var prospect = new Prospect("Scrooge", "McDuck");
        var initialCommand = new FindBestLoan(requestId, prospect, 30, 1_000_000);

        var policy = new TestableSaga<BestLoanPolicy, BestLoanData>(
            sagaFactory: () => new BestLoanPolicy(log, new FixedCreditScorer(800), new BestRateQuoteAggregator()));

        await policy.Handle(initialCommand);
        await policy.Handle(new QuoteRequestRefusedByBank(requestId, "bank1"));
        var answeringBank = "bank2";
        var interestRate = 1.5;
        await policy.Handle(new QuoteCreated(requestId, answeringBank, interestRate));
        var advanceTime = await policy.AdvanceTime(TimeSpan.FromMinutes(11));
        Assert.That(advanceTime.Length, Is.EqualTo(1));
        var handleResult = advanceTime[0];
        var replyMessage = handleResult.FindReplyMessage<BestLoanFound>();
        Assert.That(replyMessage.RequestId, Is.EqualTo(requestId));
        Assert.That(replyMessage.BankId, Is.EqualTo(answeringBank));
        Assert.That(replyMessage.InterestRate, Is.EqualTo(interestRate));
        Assert.That(handleResult.Completed, Is.True);
        Assert.That(handleResult.Context.RepliedMessages.Length, Is.EqualTo(1));
        Assert.That(handleResult.Context.PublishedMessages, Is.Empty);
        Assert.That(handleResult.Context.TimeoutMessages, Is.Empty);
        Assert.That(handleResult.Context.SentMessages, Is.Empty);
    }




    class FixedCreditScorer(int score) : ICreditScoreProvider
    {
        public int Score(Prospect prospect) => score;
    }


    static readonly ILogger<BestLoanPolicy> log = new NullLogger<BestLoanPolicy>();
}