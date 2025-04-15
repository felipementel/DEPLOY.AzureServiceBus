using DEPLOY.AzureServiceBus.WorkerService.Consumer;
using DEPLOY.AzureServiceBus.WorkerService.Consumer.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureServiceBusConfig();
builder.Services.AddOptionConfig();

//builder.Services.AddHostedService<Worker>();
//builder.Services.AddHostedService<WorkerDLQ>();
//builder.Services.AddHostedService<WorkerProcessor>();
//builder.Services.AddHostedService<WorkerPartitionSession>();
builder.Services.AddHostedService<WorkerCloudEvents>();

var host = builder.Build();

await host.RunAsync();