using BankMessages;

namespace LoanBroker.Services;

public interface IQuoteAggregator
{
    Quote Reduce(List<Quote> quotes);
}