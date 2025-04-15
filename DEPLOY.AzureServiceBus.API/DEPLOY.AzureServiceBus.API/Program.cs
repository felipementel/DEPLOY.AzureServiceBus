using DEPLOY.AzureServiceBus.API.Endpoints.v1;
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

app.MapQueueEndpointsV1();
app.MapTopicsEndpointsV1();

await app.RunAsync();

public partial class Program { }