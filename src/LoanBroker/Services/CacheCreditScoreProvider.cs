using ClientMessages;

namespace LoanBroker.Services;

class CacheCreditScoreProvider(ICreditScoreProvider delegateProvider) : ICreditScoreProvider
{
    readonly Dictionary<Prospect, (DateTime Expiration, int Score)> cache = new();

    public int Score(Prospect prospect)
    {
        if (!cache.TryGetValue(prospect, out var score) || DateTime.Now > score.Expiration)
        {
            var remoteScore = delegateProvider.Score(prospect);
            cache[prospect] = ( DateTime.Now.AddMonths(1), remoteScore);
            return remoteScore;
        }

        return cache[prospect].Score;
    }
}