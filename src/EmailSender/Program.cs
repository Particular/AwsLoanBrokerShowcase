using CommonConfigurations;
using Microsoft.Extensions.Hosting;

SharedConventions.ConfigureMicrosoftLoggingIntegration();

var builder = Host.CreateApplicationBuilder(args);

var endpointConfiguration = new EndpointConfiguration("EmailSender");

endpointConfiguration.CommonEndpointSetting();
var routingSettings = endpointConfiguration.UseCommonTransport();

// No recoverability so that errors will always go to the error queue
endpointConfiguration.Recoverability()
    .Immediate(customize => customize.NumberOfRetries(0))
    .Delayed(customize => customize.NumberOfRetries(0));

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();
app.Run();