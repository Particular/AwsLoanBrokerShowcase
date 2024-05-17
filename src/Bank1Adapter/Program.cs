using CommonConfigurations;
using Microsoft.Extensions.Hosting;

SharedConventions.ConfigureMicrosoftLoggingIntegration();

var builder = Host.CreateApplicationBuilder(args);

var endpointConfiguration = new EndpointConfiguration("Bank1Adapter");

endpointConfiguration.CommonEndpointSetting();
endpointConfiguration.UseCommonTransport();

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();
app.Run();
