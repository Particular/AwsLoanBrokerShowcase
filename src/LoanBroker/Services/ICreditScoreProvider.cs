using ClientMessages;

namespace LoanBroker.Services;

public interface ICreditScoreProvider
{
    int Score(Prospect prospect);

}