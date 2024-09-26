using LoanBroker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LoanBroker;

static class ConfigureServicesExtensions
{
    public static HostApplicationBuilder ConfigureServices(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ICreditScoreProvider>(_ => new HTTPCreditScoreProvider());
        builder.Services.AddSingleton<IQuoteAggregator, BestRateQuoteAggregator>();
        return builder;
    }
}
