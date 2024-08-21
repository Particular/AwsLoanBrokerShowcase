using ClientMessages;

namespace LoanBroker.Services;

class RandomCreditScoreProvider : ICreditScoreProvider
{
    public Task<int> Score(Prospect prospect) => Task.FromResult(Random.Shared.Next(0, 1000));
}