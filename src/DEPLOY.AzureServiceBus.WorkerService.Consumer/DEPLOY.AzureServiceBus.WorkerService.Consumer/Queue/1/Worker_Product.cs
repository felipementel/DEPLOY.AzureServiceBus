using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.WorkerService.Consumer.Domain;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class Worker_Product : BackgroundService
    {
        private readonly string _queueName = "simple-product";
        private readonly ILogger<Worker_Product> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public Worker_Product(
            ILogger<Worker_Product> logger,
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
                    _logger.LogInformation($"{_queueName}" + " at: {time}",
                        DateTimeOffset.Now);
                    Console.WriteLine(Environment.NewLine);
                }

                ServiceBusReceiver receiver = _serviceBusClient
                    .CreateReceiver(queueName: _queueName);

                ServiceBusReceivedMessage msgReceived = await receiver
                    .ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(5),
                    cancellationToken: stoppingToken);

                try
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine($"Received message: {msgReceived.Body.ToString()}");
                    Console.WriteLine(Environment.NewLine);

                    List<Product?>? product_fromJson = System.Text.Json.JsonSerializer.Deserialize<List<Product?>>
                        (msgReceived.Body.ToString(),
                        new System.Text.Json.JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNameCaseInsensitive = true,
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                            AllowTrailingCommas = true
                        });


                    List<Product?>? product_fromBinaryData = msgReceived.Body.ToObjectFromJson<List<Product?>?>
                        (new System.Text.Json.JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNameCaseInsensitive = true,
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                            AllowTrailingCommas = true
                        });

                    if (product_fromBinaryData == null || product_fromBinaryData[0]!.Quantity % 2 == 0)
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"  Even (par) Quantity: {product_fromJson![0]!.Quantity}");
                        Console.WriteLine($"  Delivery Count: {msgReceived.DeliveryCount} , Quantity {product_fromBinaryData![0]!.Quantity}");
                        Console.WriteLine(Environment.NewLine);

                        await receiver.AbandonMessageAsync(msgReceived);
                        Console.WriteLine("Message abandoned.");
                    }
                    else
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine($"  Odd (impar) Quantity: {product_fromJson![0]!.Quantity}");
                        await receiver
                            .CompleteMessageAsync(msgReceived);
                        Console.WriteLine("Message completed.");
                    }
                }
                catch (Exception ex)
                {
                    await receiver.AbandonMessageAsync(msgReceived);

                    _logger.LogError(ex, $"Error processing message {msgReceived.MessageId}");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
