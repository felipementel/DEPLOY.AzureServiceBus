using Azure.Messaging;
using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.WorkerService.Consumer.Domain;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class WorkerCloudEvents2 : BackgroundService
    {
        private readonly string _topicName = "cloud-events";
        private readonly string _subscribeName = "canal-deploy-mvp-esquenta-blumenau-2";
        private readonly ILogger<WorkerCloudEvents2> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public WorkerCloudEvents2(
            ILogger<WorkerCloudEvents2> logger,
            ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            while (!stoppingToken.IsCancellationRequested)
            {
                //if (_logger.IsEnabled(LogLevel.Information))
                //{
                //    Console.WriteLine(Environment.NewLine);
                //    _logger.LogInformation("Cloud Events at: {time}",
                //        DateTimeOffset.Now);
                //    Console.WriteLine(Environment.NewLine);
                //}

                ServiceBusProcessor processor = _serviceBusClient.CreateProcessor(
                _topicName,
                _subscribeName,
                new ServiceBusProcessorOptions
                {
                    PrefetchCount = 1,
                    AutoCompleteMessages = false
                });

                processor.ProcessMessageAsync += async args =>
                {
                    CloudEvent? receivedCloudEvent = CloudEvent.Parse(args.Message.Body);
                    Product? receivedProduct = receivedCloudEvent!.Data!.ToObjectFromJson<Product>();

                    Console.WriteLine($"Subscriber: {_subscribeName}");
                    Console.WriteLine($"  Data: {receivedCloudEvent.Data}");
                    Console.WriteLine($"  Subject: {receivedCloudEvent.Subject}");
                    Console.WriteLine($"  Type: {receivedCloudEvent.Type}");
                    Console.WriteLine($"  Source: {receivedCloudEvent.Source}");
                    Console.WriteLine($"  ID: {receivedCloudEvent.Id}");
                    Console.WriteLine($"  DataContentType: {receivedCloudEvent.DataContentType}");
                    Console.WriteLine($"  DataSchema: {receivedCloudEvent.DataSchema}");
                    Console.WriteLine($"  Time: {receivedCloudEvent.Time}");

                    await args.CompleteMessageAsync(args.Message);
                };

                processor.ProcessErrorAsync += args =>
                {
                    _logger.LogError(args.Exception, "Error processing message.");
                    return Task.CompletedTask;
                };

                await processor.StartProcessingAsync();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                await processor.StopProcessingAsync(stoppingToken);
            }
        }
    }
}
