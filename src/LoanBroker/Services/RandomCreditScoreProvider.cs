using ClientMessages;

namespace LoanBroker.Services;

class RandomCreditScoreProvider : ICreditScoreProvider
{
    readonly Random _rnd = new (42);
    public int Score(Prospect prospect) => _rnd.Next(0, 1000);
}