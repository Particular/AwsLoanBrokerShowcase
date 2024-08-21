using ClientMessages;

namespace LoanBroker.Services;

class RandomCreditScoreProvider : ICreditScoreProvider
{
    public Task<int> Score(Prospect prospect, string requestId) => Task.FromResult(Random.Shared.Next(0, 1000));
}