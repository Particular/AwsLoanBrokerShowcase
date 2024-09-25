using System.Diagnostics.Metrics;

namespace LoanBroker;

static class CustomTelemetry
{
    static readonly Meter LoanBrokerMeter = new("LoanBroker", "0.1.0");

    public static readonly Histogram<double> BankResponseTime = LoanBrokerMeter.CreateHistogram<double>(
        name: "loan_broker.bank_processing_time",
        unit: "s",
        description: "The time banks take to respond to quote requests.");

}
