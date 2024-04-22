namespace Handlers;

using Messages;

public interface IQuoteAggregator
{

    Quote Reduce(Dictionary<string, double> quotes);

}