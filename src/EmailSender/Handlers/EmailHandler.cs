using ClientMessages;
using Microsoft.Extensions.Logging;

namespace EmailSender.Handlers;

public class EmailHandler(ILogger<EmailHandler> logger) : IHandleMessages<BestLoanFound>
{
    public Task Handle(BestLoanFound message, IMessageHandlerContext context)
    {
        // Throw exceptions 5% of the time, sending message to error queue
        if (Random.Shared.Next(0, 100) < 5)
        {
            throw new Exception("Simulated email failure");
        }

        logger.LogInformation("Sending email for Request {0} for {1}% offer from {2}",
            message.RequestId, message.InterestRate, message.BankId);

        return Task.CompletedTask;
    }
}
