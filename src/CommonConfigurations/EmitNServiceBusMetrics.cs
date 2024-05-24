using System.Diagnostics;
using System.Diagnostics.Metrics;
using NServiceBus.Features;
using NServiceBus.Pipeline;

namespace CommonConfigurations;

public class EmitNServiceBusMetrics : Feature
{
    public static readonly Meter NServiceBusMeter = new("NServiceBus.Core", "0.1.0");

    public EmitNServiceBusMetrics()
    {
        EnableByDefault();
    }

    protected override void Setup(FeatureConfigurationContext context)
    {
        var queueName = context.LocalQueueAddress().BaseAddress;
        var discriminator = context.InstanceSpecificQueueAddress()?.Discriminator;

        context.Pipeline.OnReceivePipelineCompleted((e, _) =>
        {
            e.TryGetMessageType(out var messageType);
            e.TryGetRequestId(out var requestId);

            var tags = new TagList(
            [
                new(Tags.QueueName, queueName ?? ""),
                new(Tags.EndpointDiscriminator, discriminator ?? ""),
                new(Tags.MessageType, messageType ?? ""),
                //new(Tags.LoanBrokerRequestId, requestId ?? "")
            ]);

            ProcessingTime.Record((e.CompletedAt - e.StartedAt).TotalMilliseconds, tags);

            if (e.TryGetDeliverAt(out var startTime) || e.TryGetTimeSent(out startTime))
            {
                CriticalTime.Record((e.CompletedAt - startTime).TotalMilliseconds, tags);
            }

            return Task.CompletedTask;
        });

        context.Pipeline.Register(new RecordHandlerTimeMetric(queueName), "Record the handler execution time metric.");

    }


    static readonly Histogram<double> ProcessingTime =
        NServiceBusMeter.CreateHistogram<double>("nservicebus.messaging.processingtime", "ms",
            "The time in milliseconds between when the message was pulled from the queue until processed by the endpoint.");

    static readonly Histogram<double> CriticalTime =
        NServiceBusMeter.CreateHistogram<double>("nservicebus.messaging.criticaltime", "ms",
            "The time in milliseconds between when the message was sent until processed by the endpoint.");

    static class Tags
    {
        public const string EndpointDiscriminator = "nservicebus.discriminator";
        public const string QueueName = "nservicebus.queue";
        public const string MessageType = "nservicebus.message_type";
        public const string LoanBrokerRequestId = "loan_broker.request_id";
    }
}

class RecordHandlerTimeMetric(string queueName) : Behavior<IInvokeHandlerContext>
{
    public override Task Invoke(IInvokeHandlerContext context, Func<Task> next)
    {
        var start = DateTime.UtcNow;
        var handlerType = context.MessageHandler.Instance.GetType();
        var messageType = context.MessageBeingHandled.GetType();
        return next().ContinueWith(t =>
        {
            var tags = new TagList(
            [
                new(Tags.MessageHandler, handlerType),
                new(Tags.QueueName, queueName ),
                new(Tags.MessageType, messageType )

            ]);
            HandlerTime.Record((DateTime.UtcNow - start).TotalMilliseconds, tags);
            if (t.IsFaulted)
            {
                throw t.Exception;
            }
        }, context.CancellationToken);
    }

    static class Tags
    {
        public const string MessageHandler = "nservicebus.message_handler";
        public const string QueueName = "nservicebus.queue";
        public const string MessageType = "nservicebus.message_type";
    }

    static readonly Histogram<double> HandlerTime =
        EmitNServiceBusMetrics.NServiceBusMeter.CreateHistogram<double>("nservicebus.messaging.handlerTime", "ms",
            "The time in milliseconds for the execution of the business code.");
}