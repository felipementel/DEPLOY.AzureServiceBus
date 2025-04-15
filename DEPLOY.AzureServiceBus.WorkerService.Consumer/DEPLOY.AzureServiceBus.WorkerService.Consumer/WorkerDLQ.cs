using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class WorkerDLQ : BackgroundService
    {
        private readonly ILogger<WorkerDLQ> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public WorkerDLQ(
            ILogger<WorkerDLQ> logger,
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
                    _logger.LogInformation("WorkerDLQ running at: {time}",
                        DateTimeOffset.Now);
                    Console.WriteLine(Environment.NewLine);
                }

                ServiceBusReceiver receiver = _serviceBusClient
                    .CreateReceiver(queueName: "simple",
                    new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter });

                ServiceBusReceivedMessage dlqMessage = await receiver
                    .ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(5),
                    cancellationToken: stoppingToken);

                try
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine($"Received message from DLQ: {dlqMessage.Body.ToString()}");

                    Console.WriteLine($"   DeadLetterSource: {dlqMessage.DeadLetterSource}");
                    Console.WriteLine($"   DeadLetterReason: {dlqMessage.DeadLetterReason}");
                    Console.WriteLine($"   DeadLetterErrorDescription: {dlqMessage.DeadLetterErrorDescription}");
                    Console.WriteLine(Environment.NewLine);
                    await receiver
                        .CompleteMessageAsync(dlqMessage);
                }
                catch (Exception ex)
                {
                    await receiver.AbandonMessageAsync(dlqMessage);

                    _logger.LogError(ex, $"Error processing message from DLQ {dlqMessage.MessageId}");
                }

                //Create a event
                if (dlqMessage.ReplyTo != null)
                {
                    ServiceBusSender sender = _serviceBusClient.CreateSender(dlqMessage.ReplyTo);

                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString("Produto Cadastrado com sucesso."),
                        SessionId = dlqMessage.ReplyToSessionId,
                        ApplicationProperties =
                    {
                        ["ReplyTo"] = dlqMessage.ReplyTo,
                        ["SessionId"] = dlqMessage.SessionId,
                        ["MessageId"] = dlqMessage.MessageId
                    }
                    }, stoppingToken);

                }
                else
                {
                    Console.WriteLine(Environment.NewLine);
                    _logger.LogWarning($"Message {dlqMessage.MessageId} has no ReplyTo property.");
                    Console.WriteLine(Environment.NewLine);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
