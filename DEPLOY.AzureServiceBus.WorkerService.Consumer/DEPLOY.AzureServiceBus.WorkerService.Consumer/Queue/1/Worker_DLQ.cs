using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class Worker_DLQ : BackgroundService
    {
        private readonly string _queueName = "simple"; //dlq name is the same as the queue name
        private readonly ILogger<Worker_DLQ> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public Worker_DLQ(
            ILogger<Worker_DLQ> logger,
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
                    _logger.LogInformation($"{_queueName}" + " at: {time}",
                        DateTimeOffset.Now);
                    Console.WriteLine(Environment.NewLine);
                }

                ServiceBusReceiver receiver = _serviceBusClient
                    .CreateReceiver(queueName: _queueName,
                    new ServiceBusReceiverOptions
                    {
                        SubQueue = SubQueue.DeadLetter
                    });

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

                    if (dlqMessage.DeadLetterReason == "Duplicate")
                    {
                        await receiver.AbandonMessageAsync(dlqMessage);
                        Console.WriteLine($"Message {dlqMessage.MessageId} is a duplicate and was abandoned.");
                    }
                    else
                    {
                        await receiver.CompleteMessageAsync(dlqMessage);
                        Console.WriteLine($"Message {dlqMessage.MessageId} was completed.");
                    }
                    if (dlqMessage.ReplyTo != null)
                    {
                        Console.WriteLine($"ReplyTo: {dlqMessage.ReplyTo}");
                        Console.WriteLine($"SessionId: {dlqMessage.ReplyToSessionId}");

                        mustReply = true;
                    }
                    else
                    {
                        Console.WriteLine($"Message {dlqMessage.MessageId} has no ReplyTo property.");
                    }
                }
                catch (Exception ex)
                {
                    await receiver.AbandonMessageAsync(dlqMessage);

                    _logger.LogError(ex, $"Error processing message from DLQ {dlqMessage.MessageId}");
                }

                //Create a event
                if (mustReply)
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
