using BankMessages;

namespace LoanBroker.Services;

class BestRateQuoteAggregator : IQuoteAggregator
{
    public Quote Reduce(IEnumerable<Quote> quotes) => quotes.OrderBy(q => q.InterestRate).First();
}