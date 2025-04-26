using DEPLOY.AzureServiceBus.WorkerService.Consumer;
using DEPLOY.AzureServiceBus.WorkerService.Consumer.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureServiceBusConfig();
builder.Services.AddOptionConfig();

Console.Clear();

// 1
builder.Services.AddHostedService<Worker_Product_001>();
//builder.Services.AddHostedService<Worker_Duplicate_002>();
//builder.Services.AddHostedService<Worker_Schedule_003>();
//builder.Services.AddHostedService<Worker_Simple_Qtd_004>();
//builder.Services.AddHostedService<Worker_Batch_Normal_005>();
//builder.Services.AddHostedService<Worker_Batch_Processor_006>();

//builder.Services.AddHostedService<WorkerDLQ>();

// partition
builder.Services.AddHostedService<Worker_Processor_Partition>();

//session
builder.Services.AddHostedService<Worker_Processor_Partition_Session>();

// Topic
//builder.Services.AddHostedService<WorkerCloudEvents>();
//builder.Services.AddHostedService<WorkerCloudEvents2>();

var host = builder.Build();

await host.RunAsync();
