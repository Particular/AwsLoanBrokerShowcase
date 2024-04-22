using Messages;

namespace LoanBroker.Services;

public interface IQuoteAggregator
{

    Quote Reduce(Dictionary<string, double> quotes);

}