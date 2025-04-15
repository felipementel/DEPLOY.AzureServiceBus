using Azure.Messaging;
using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class WorkerCloudEvents : BackgroundService
    {
        private readonly ILogger<WorkerCloudEvents> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public WorkerCloudEvents(
            ILogger<WorkerCloudEvents> logger,
            ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            bool mustReply = false;

            while (!stoppingToken.IsCancellationRequested)
            {
                //if (_logger.IsEnabled(LogLevel.Information))
                //{
                //    Console.WriteLine(Environment.NewLine);
                //    _logger.LogInformation("Cloud Events at: {time}",
                //        DateTimeOffset.Now);
                //    Console.WriteLine(Environment.NewLine);
                //}

                ServiceBusSessionProcessor processor = _serviceBusClient.CreateSessionProcessor(
                "cloud-events",
                "canal-deploy-mvp-esquenta-blumenau",
                new ServiceBusSessionProcessorOptions
                {
                    MaxConcurrentSessions = 5, // Número máximo de sessões processadas simultaneamente
                    PrefetchCount = 1,
                    AutoCompleteMessages = false
                });

                processor.ProcessMessageAsync += async args =>
                {
                    CloudEvent? receivedCloudEvent = CloudEvent.Parse(args.Message.Body);
                    Product? receivedProduct = receivedCloudEvent!.Data!.ToObjectFromJson<Product>();

                    Console.WriteLine(receivedProduct!.Name);
                    Console.WriteLine(receivedProduct.Quantity);

                    await args.CompleteMessageAsync(args.Message);
                };

                processor.ProcessErrorAsync += args =>
                {
                    _logger.LogError(args.Exception, "Error processing message.");
                    return Task.CompletedTask;
                };

                await processor.StartProcessingAsync();


                //CloudEvent? receivedCloudEvent = CloudEvent.Parse(receivedMessage.Body);

                //Product? receivedProduct = receivedCloudEvent!.Data!.ToObjectFromJson<Product>();

                //Console.WriteLine(receivedProduct!.Name);
                //Console.WriteLine(receivedProduct.Quantity);

                //await receiver.CompleteMessageAsync(receivedMessage);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
