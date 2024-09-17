using System.Diagnostics;
using BankMessages;
using ClientMessages;
using LoanBroker.Messages;
using LoanBroker.Policies;
using LoanBroker.Services;
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
        var prospect = new Prospect("Scrooge", "McDuck", "123-45-6789");

        var initialCommand = new FindBestLoanWithScore(requestId, prospect, 30, 1_000_000, 800);

        var policy = new TestableSaga<BestLoanPolicy, BestLoanData>(
            sagaFactory: () => new BestLoanPolicy(log));

        var result1 = await policy.Handle(initialCommand);
        var quoteRequested = result1.FindPublishedMessage<QuoteRequested>();

        Assert.That(quoteRequested, Is.Not.Null);
        Assert.That(quoteRequested.RequestId, Is.EqualTo(requestId));
        Assert.That(quoteRequested.NumberOfYears, Is.EqualTo(30));
        Assert.That(quoteRequested.Score, Is.EqualTo(800));

        var timeoutRequested = result1.FindTimeoutMessage<MaxTimeout>();
        Assert.That(timeoutRequested, Is.Not.Null);

        (string BankId, double InterestRate)[] bankResponses =
        [
            new("FirstNational", 3.05),
            new("SecondRegional", 2.95),
            new("AreYouKidding", 99.99)
        ];

        foreach (var response in bankResponses)
        {
            var quoteResult = await policy.Handle(new QuoteCreated(requestId, response.BankId, response.InterestRate));
            Assert.That(quoteResult.Completed, Is.False);
            Assert.That(quoteResult.Context.SentMessages, Is.Empty);
            Assert.That(quoteResult.Context.PublishedMessages, Is.Empty);
            Assert.That(quoteResult.Context.TimeoutMessages, Is.Empty);
            Debug.Assert(quoteResult.SagaDataSnapshot.BestQuoteInterestRate > 0, "quoteResult.SagaDataSnapshot.BestQuoteInterestRate > 0");
            Debug.Assert(quoteResult.SagaDataSnapshot.BestQuoteBankId != null, "quoteResult.SagaDataSnapshot.BestQuoteBankId != null");
        }

        var timeoutResults = await policy.AdvanceTime(TimeSpan.FromMinutes(10));

        Assert.That(timeoutResults.Length, Is.EqualTo(1));
        var onlyResult = timeoutResults.First();
        var publishedMessage = onlyResult.FindPublishedMessage<BestLoanFound>();

        Assert.That(publishedMessage, Is.Not.Null);
        Assert.That(publishedMessage.RequestId, Is.EqualTo(requestId));
        Assert.That(publishedMessage.BankId, Is.EqualTo("SecondRegional"));
        Assert.That(publishedMessage.InterestRate, Is.EqualTo(2.95));
    }

    [Test]
    public async Task NoResponsesFromBanks()
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        var prospect = new Prospect("Scrooge", "McDuck", "123-45-6789");
        var initialCommand = new FindBestLoanWithScore(requestId, prospect, 30, 1_000_000,800);

        var policy = new TestableSaga<BestLoanPolicy, BestLoanData>(
            sagaFactory: () => new BestLoanPolicy(log));

        await policy.Handle(initialCommand);
        var advanceTime = await policy.AdvanceTime(TimeSpan.FromMinutes(11));
        Assert.That(advanceTime.Length, Is.EqualTo(1));
        var handleResult = advanceTime[0];
        var publishedMessage = handleResult.FindPublishedMessage<NoQuotesReceived>();
        Assert.That(publishedMessage.RequestId, Is.EqualTo(requestId));
        Assert.That(handleResult.Completed, Is.True);
        Assert.That(handleResult.Context.PublishedMessages.Length, Is.EqualTo(1));
        Assert.That(handleResult.Context.RepliedMessages, Is.Empty);
        Assert.That(handleResult.Context.TimeoutMessages, Is.Empty);
        Assert.That(handleResult.Context.SentMessages, Is.Empty);
    }

    [Test]
    public async Task LoanRequestRefusedByAllBanks()
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        var prospect = new Prospect("Scrooge", "McDuck", "123-45-6789");
        var initialCommand = new FindBestLoanWithScore(requestId, prospect, 30, 1_000_000, 800);

        var policy = new TestableSaga<BestLoanPolicy, BestLoanData>(
            sagaFactory: () => new BestLoanPolicy(log));

        await policy.Handle(initialCommand);
        await policy.Handle(new QuoteRequestRefusedByBank(requestId, "bank1"));
        var advanceTime = await policy.AdvanceTime(TimeSpan.FromMinutes(11));
        Assert.That(advanceTime.Length, Is.EqualTo(1));
        var handleResult = advanceTime[0];
        var publishedMessage = handleResult.FindPublishedMessage<QuoteRequestRefused>();
        Assert.That(publishedMessage.RequestId, Is.EqualTo(requestId));
        Assert.That(handleResult.Completed, Is.True);
        Assert.That(handleResult.Context.RepliedMessages, Is.Empty);
        Assert.That(handleResult.Context.PublishedMessages.Length, Is.EqualTo(1));
        Assert.That(handleResult.Context.TimeoutMessages, Is.Empty);
        Assert.That(handleResult.Context.SentMessages, Is.Empty);
    }

    [Test]
    public async Task LoanRequestRefusedByOneBankAcceptedByAnother()
    {
        var requestId = Guid.NewGuid().ToString()[..8];
        var prospect = new Prospect("Scrooge", "McDuck", "123-45-6789");
        var initialCommand = new FindBestLoanWithScore(requestId, prospect, 30, 1_000_000, 800);

        var policy = new TestableSaga<BestLoanPolicy, BestLoanData>(
            sagaFactory: () => new BestLoanPolicy(log));

        await policy.Handle(initialCommand);
        await policy.Handle(new QuoteRequestRefusedByBank(requestId, "bank1"));
        const string answeringBank = "bank2";
        const double interestRate = 1.5;
        await policy.Handle(new QuoteCreated(requestId, answeringBank, interestRate));
        var advanceTime = await policy.AdvanceTime(TimeSpan.FromMinutes(11));
        Assert.That(advanceTime.Length, Is.EqualTo(1));
        var handleResult = advanceTime[0];
        var publishedMessage = handleResult.FindPublishedMessage<BestLoanFound>();
        Assert.That(publishedMessage.RequestId, Is.EqualTo(requestId));
        Assert.That(publishedMessage.BankId, Is.EqualTo(answeringBank));
        Assert.That(publishedMessage.InterestRate, Is.EqualTo(interestRate));
        Assert.That(handleResult.Completed, Is.True);
        Assert.That(handleResult.Context.PublishedMessages.Length, Is.EqualTo(1));
        Assert.That(handleResult.Context.RepliedMessages, Is.Empty);
        Assert.That(handleResult.Context.TimeoutMessages, Is.Empty);
        Assert.That(handleResult.Context.SentMessages, Is.Empty);
    }

    class FixedCreditScorer(int score) : ICreditScoreProvider
    {
        public Task<int> Score(Prospect prospect, string requestId) => Task.FromResult(score);
    }

    static readonly ILogger<BestLoanPolicy> log = new NullLogger<BestLoanPolicy>();
}