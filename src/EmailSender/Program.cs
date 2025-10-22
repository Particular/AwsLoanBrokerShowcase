using CommonConfigurations;
using Microsoft.Extensions.Hosting;

// No recoverability so that errors will always go to the error queue
Host.CreateApplicationBuilder(args)
    .ConfigureNServiceBusEndpoint("EmailSender", c => c.EndpointConfiguration.DisableRetries())
    .Build()
    .Run();