using DEPLOY.AzureServiceBus.API.Endpoints.v1;
using DEPLOY.AzureServiceBus.API.Endpoints.v2;
using DEPLOY.AzureServiceBus.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApiConfig();
builder.Services.AddOptionConfig();

builder.Services.AddAzureServiceBusConfig();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApiConfig();
}
else
{
    app.UseHttpsRedirection();
}

//Queue (v1)
app.MapQueueEndpointsV1();
app.MapQueuePartitionEndpointsV1();
app.MapQueueSessionEndpointsV1();

//Topic (v2)
app.MapTopicsEndpointsV2();
app.MapTopicsCloudEventsEndpointsV2

await app.RunAsync();

public partial class Program { }
