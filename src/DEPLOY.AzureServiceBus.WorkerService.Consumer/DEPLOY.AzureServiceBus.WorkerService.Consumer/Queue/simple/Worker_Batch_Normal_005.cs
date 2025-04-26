using Azure.Messaging.ServiceBus;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class Worker_Batch_Normal_005 : BackgroundService
    {
        private readonly string _queueName = "simple-batch";
        private readonly ILogger<Worker_Batch_Normal_005> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public Worker_Batch_Normal_005(
            ILogger<Worker_Batch_Normal_005> logger,
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
                ServiceBusReceiver receiver = _serviceBusClient
                    .CreateReceiver(queueName: _queueName);

                ServiceBusReceivedMessage msgReceived = await receiver
                    .ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(5),
                    cancellationToken: stoppingToken);

                try
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine($"Received message: {msgReceived.Body.ToString()}");

                    var product = msgReceived.Body.ToString();

                    Console.WriteLine($"   Product: {product}");
                    Console.WriteLine(Environment.NewLine);

                    //Processamento qualquer, exemplo: Banco de dados

                    mustReply = true;
                    await receiver
                        .CompleteMessageAsync(msgReceived);
                }
                catch (Exception ex)
                {
                    await receiver.AbandonMessageAsync(msgReceived);

                    _logger.LogError(ex, $"Error processing message {msgReceived.MessageId}");
                }

                if (mustReply && msgReceived.ReplyTo is not null)
                {
                    ServiceBusSender sender = _serviceBusClient.CreateSender(msgReceived.ReplyTo);

                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString("Produto Cadastrado com sucesso."),
                        ApplicationProperties =
                        {
                            ["ProcessedBy"] = "CanalDEPLOY-ProcessorOne"
                        }
                    }, stoppingToken);

                    Console.WriteLine($"   Sent event to {msgReceived.ReplyTo}");
                }
                else
                {
                    _logger.LogWarning($"Message {msgReceived.MessageId} has no ReplyTo property.");
                }

                //await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
