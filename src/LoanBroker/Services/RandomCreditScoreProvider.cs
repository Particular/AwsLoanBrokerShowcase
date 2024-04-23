using Messages;

namespace LoanBroker.Services;

class RandomCreditScoreProvider : ICreditScoreProvider
{
    readonly Random rnd = new (42);
    public int Score(Prospect prospect) => rnd.Next(0, 100);
}