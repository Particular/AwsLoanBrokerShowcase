using BankMessages;

namespace LoanBroker.Services;

internal class BestRateQuoteAggregator : IQuoteAggregator
{
    public Quote Reduce(IEnumerable<Quote> quotes) => quotes.OrderBy(q => q.InterestRate).First();
}