namespace LoanBroker.Services;

class CreditScoreProvider : ICreditScoreProvider
{
    public int Score()
    {
        return 42;
    }
}