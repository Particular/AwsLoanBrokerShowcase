using Microsoft.Extensions.Logging;

namespace Handlers;

using Messages;

internal class BestLoanPolicy(ILogger<BestLoanPolicy> log) : Saga<BestLoanData>,
    IAmStartedByMessages<FindBestLoan>,
    IAmStartedByMessages<QuoteCreated>,
    IHandleTimeouts<MyCustomTimeout>
{
    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<BestLoanData> mapper)
    {
        // https://docs.particular.net/nservicebus/sagas/message-correlation
        mapper.MapSaga(saga => saga.CorrelationId)
            .ToMessage<FindBestLoan>(message => message.CorrelationId)
            .ToMessage<QuoteCreated>(message => message.CorrelationId);
    }

    public async Task Handle(FindBestLoan message, IMessageHandlerContext context)
    {
        // Business logic here
        // http request score provider
        // publish (QuoteRequested)
        // request timeout
    }

    public async Task Handle(QuoteCreated message, IMessageHandlerContext context)
    {
        
        // save
        
        // Update saga data: https://docs.particular.net/nservicebus/sagas/#long-running-means-stateful
        // this.Data.Property = ...

        // Sending commands: https://docs.particular.net/nservicebus/messaging/send-a-message#inside-the-incoming-message-processing-pipeline
        // await context.Send(...);

        // Publishing events https://docs.particular.net/nservicebus/messaging/publish-subscribe/publish-handle-event
        // await context.Publish(...);

        // Request a timeout: https://docs.particular.net/nservicebus/sagas/timeouts
        // await RequestTimeout<MyCustomTimeout>(context, TimeSpan.FromMinutes(10));

        // Ending a saga: https://docs.particular.net/nservicebus/sagas/#ending-a-saga
        // MarkAsComplete();
    }

    public async Task Timeout(MyCustomTimeout timeout, IMessageHandlerContext context)
    {
        
        // Remove if saga does not require timeouts
        // reduce
        // publish / send(reply)
    }
    
    
}

internal class BestLoanData : ContainSagaData
{
    public string CorrelationId { get; set; }
    // Other properties
}

internal class MyCustomTimeout
{
    // Optional extra properties
}
