using Messages;

namespace LoanBroker.Services;

public interface ICreditScoreProvider
{
    int Score(Prospect prospect);

}