using Azure.Messaging.ServiceBus;
using System.Text;
using System.Transactions;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class WorkerTransaction : BackgroundService
    {
        const string queueName = "simple-transaction";
        private readonly ILogger<WorkerTransaction> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public WorkerTransaction(
            ILogger<WorkerTransaction> logger,
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
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    Console.WriteLine(Environment.NewLine);
                    _logger.LogInformation("Transaction at: {time}",
                        DateTimeOffset.Now);
                    Console.WriteLine(Environment.NewLine);
                }

                ServiceBusSender sender = _serviceBusClient.CreateSender(queueName);

                await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes("First")));

                ServiceBusReceiver receiver = _serviceBusClient.CreateReceiver(queueName);
                ServiceBusReceivedMessage firstMessage = await receiver.ReceiveMessageAsync();

                using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await sender.SendMessageAsync(new ServiceBusMessage(Encoding.UTF8.GetBytes("Second")));

                    await receiver.CompleteMessageAsync(firstMessage);

                    ts.Complete();
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
