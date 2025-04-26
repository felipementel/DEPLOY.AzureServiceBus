using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class Worker_Schedule : BackgroundService
    {
        private readonly string _queueName = "simple-schedule";
        private readonly ILogger<Worker_Schedule> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public Worker_Schedule(
            ILogger<Worker_Schedule> logger,
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
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    Console.WriteLine(Environment.NewLine);
                    _logger.LogInformation("Duplicate at: {time}",
                        DateTimeOffset.Now);
                    Console.WriteLine(Environment.NewLine);
                }

                ServiceBusReceiver receiver = _serviceBusClient
                    .CreateReceiver(queueName: _queueName, new ServiceBusReceiverOptions()
                    {

                    });

                ServiceBusReceivedMessage msgReceived = await receiver
                    .ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(5),
                    cancellationToken: stoppingToken);

                Console.WriteLine(Environment.NewLine);
                Console.WriteLine($"Received message: {msgReceived.Body.ToString()}");
                Console.WriteLine(Environment.NewLine);

                await receiver.CompleteMessageAsync(msgReceived);

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
