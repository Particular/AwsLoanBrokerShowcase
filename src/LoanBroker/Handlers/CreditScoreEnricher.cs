using ClientMessages;
using LoanBroker.Messages;
using LoanBroker.Services;
using Microsoft.Extensions.Logging;

namespace LoanBroker.Handlers;

[Handler]
public class CreditScoreEnricher(ILogger<CreditScoreEnricher> logger,
    ICreditScoreProvider creditScoreProvider) : IHandleMessages<FindBestLoan>
{
    public async Task Handle(FindBestLoan message, IMessageHandlerContext context)
    {
        logger.LogInformation($"FindBestLoan request received from {message.Prospect}, with ID {message.RequestId}. Details: number of years {message.NumberOfYears}, amount: {message.Amount}");
        var score = await creditScoreProvider.Score(message.Prospect, message.RequestId);
        var findBestLoanWithScore = new FindBestLoanWithScore(message.RequestId, message.Prospect, message.NumberOfYears, message.Amount, score);
        await context.Send(findBestLoanWithScore);
    }
}