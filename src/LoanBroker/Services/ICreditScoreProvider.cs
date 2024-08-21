using ClientMessages;

namespace LoanBroker.Services;

public interface ICreditScoreProvider
{
    Task<int> Score(Prospect prospect);

}