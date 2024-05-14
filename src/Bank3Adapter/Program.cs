using CommonConfigurations;
using Microsoft.Extensions.Hosting;

// To create a docker container, use the following command: dotnet publish /t:PublishContainer
// See https://learn.microsoft.com/en-us/dotnet/core/docker/publish-as-container?pivots=dotnet-8-0#publish-net-app for details

var builder = Host.CreateApplicationBuilder(args);

var endpointConfiguration = new EndpointConfiguration("Bank3Adapter");

endpointConfiguration.CommonEndpointSetting();
endpointConfiguration.UseCommonTransport();

builder.UseNServiceBus(endpointConfiguration);

var app = builder.Build();
app.Run();