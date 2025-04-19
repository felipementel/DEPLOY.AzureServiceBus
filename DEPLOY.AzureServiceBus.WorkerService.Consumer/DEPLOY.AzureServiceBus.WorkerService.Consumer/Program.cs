using DEPLOY.AzureServiceBus.WorkerService.Consumer;
using DEPLOY.AzureServiceBus.WorkerService.Consumer.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureServiceBusConfig();
builder.Services.AddOptionConfig();

Console.Clear();

// 1
//builder.Services.AddHostedService<Worker_Product>();
//builder.Services.AddHostedService<Worker_Batch>();
//builder.Services.AddHostedService<Worker_Batch_Processor>();
//builder.Services.AddHostedService<Worker_Duplicate>();
//builder.Services.AddHostedService<WorkerDLQ>();

// 2
//builder.Services.AddHostedService<Worker_Processor_Partition>();
//builder.Services.AddHostedService<Worker_Processor_Partition_Session>();

// Topic
builder.Services.AddHostedService<WorkerCloudEvents>();
builder.Services.AddHostedService<WorkerCloudEvents2>();

var host = builder.Build();

await host.RunAsync();
