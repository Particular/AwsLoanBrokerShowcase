using ClientMessages;

namespace LoanBroker.Services;

internal class RandomCreditScoreProvider : ICreditScoreProvider
{
    private readonly Random _rnd = new (42);
    public int Score(Prospect prospect) => _rnd.Next(0, 1000);
}