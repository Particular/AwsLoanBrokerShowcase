using BankMessages;

namespace LoanBroker.Services;

public interface IQuoteAggregator
{
    Quote Reduce(IEnumerable<Quote> quotes);
}