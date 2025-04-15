using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public Worker(
            ILogger<Worker> logger,
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
                    _logger.LogInformation("Simple at: {time}",
                        DateTimeOffset.Now);
                    Console.WriteLine(Environment.NewLine);
                }

                ServiceBusReceiver receiver = _serviceBusClient
                    .CreateReceiver(queueName: "simple");

                ServiceBusReceivedMessage msgReceived = await receiver
                    .ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(5),
                    cancellationToken: stoppingToken);

                try
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine($"Received message: {msgReceived.Body.ToString()}");
                    Console.WriteLine(Environment.NewLine);

                    // Process the message here
                    List<Product?>? product1 = System.Text.Json.JsonSerializer.Deserialize<List<Product?>>(msgReceived.Body.ToString(), new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        AllowTrailingCommas = true
                    });


                    List<Product?>? product = msgReceived.Body.ToObjectFromJson<List<Product?>?>(new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        AllowTrailingCommas = true
                    });

                    if (product == null || product[0]!.Quantity % 2 == 0)
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"  Delivery Count: {msgReceived.DeliveryCount}");
                        Console.WriteLine(Environment.NewLine);

                        await receiver.AbandonMessageAsync(msgReceived);
                    }
                    else
                    {
                        mustReply = true;
                        await receiver
                            .CompleteMessageAsync(msgReceived);
                    }
                }
                catch (Exception ex)
                {
                    await receiver.AbandonMessageAsync(msgReceived);

                    //_logger.LogError(ex, $"Error processing message {msgReceived.MessageId}");
                }

                if (mustReply && msgReceived.ReplyTo is not null)
                {
                    ServiceBusSender sender = _serviceBusClient.CreateSender(msgReceived.ReplyTo);

                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString("Produto Cadastrado com sucesso."),
                        SessionId = msgReceived.ReplyToSessionId,
                        MessageId = msgReceived.CorrelationId, // ATENCAOs
                        ApplicationProperties =
                    {
                        ["ProcessedBy"] = "CanalDEPLOY-ProcessorOne"
                    }
                    }, stoppingToken);

                    Console.WriteLine($"   Sent event to {msgReceived.ReplyTo}");
                }
                else
                {
                    //_logger.LogWarning($"Message {msgReceived.MessageId} has no ReplyTo property.");
                }

                

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
