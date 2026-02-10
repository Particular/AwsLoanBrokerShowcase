using CreditBureau;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.UseMiddleware<RequestLoggingMiddleware>();
    })
    .Build();

host.Run();
