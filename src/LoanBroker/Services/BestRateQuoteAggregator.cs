using BankMessages;

namespace LoanBroker.Services;

class BestRateQuoteAggregator : IQuoteAggregator
{
    public Quote Reduce(List<Quote> quotes) => quotes.OrderBy(q => q.InterestRate).First();
}