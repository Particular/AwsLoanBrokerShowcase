using ClientMessages;

namespace LoanBroker.Services;

class CacheCreditScoreProvider(ICreditScoreProvider delegateProvider) : ICreditScoreProvider
{
    readonly Dictionary<Prospect, (DateTime Expiration, int Score)> _cache = new();

    public int Score(Prospect prospect)
    {
        if (!_cache.TryGetValue(prospect, out var score) || DateTime.Now > score.Expiration)
        {
            var remoteScore = delegateProvider.Score(prospect);
            _cache[prospect] = ( DateTime.Now.AddMonths(1), remoteScore);
            return remoteScore;
        }

        return _cache[prospect].Score;
    }
}