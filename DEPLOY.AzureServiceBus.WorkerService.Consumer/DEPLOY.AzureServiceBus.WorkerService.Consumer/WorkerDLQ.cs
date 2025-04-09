using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Amqp.Framing;
using System;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class WorkerDLQ : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusClient _serviceBusClient;

        public WorkerDLQ(
            ILogger<Worker> logger,
            ServiceBusClient serviceBusClient)
        {
            _logger = logger;
            _serviceBusClient = serviceBusClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("WorkerDLQ running at: {time}",
                        DateTimeOffset.Now);
                }

                ServiceBusReceiver receiver = _serviceBusClient
                    .CreateReceiver(queueName: "simple",
                    new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter });

                ServiceBusReceivedMessage msg = await receiver
                    .ReceiveMessageAsync(maxWaitTime: TimeSpan.FromMinutes(5),
                    cancellationToken: stoppingToken);

                try
                {
                    Console.WriteLine($"Received message from DLQ: {msg.Body.ToString()}");
                    // Process the message here

                    await receiver
                        .CompleteMessageAsync(msg);
                }
                catch (Exception ex)
                {
                    await receiver.AbandonMessageAsync(msg);

                    _logger.LogError(ex, $"Error processing message from DLQ {msg.MessageId}");
                }

                //Create a event
                if (msg.ReplyTo != null)
                {
                    ServiceBusSender sender = _serviceBusClient.CreateSender(msg.ReplyTo);

                    await sender.SendMessageAsync(new ServiceBusMessage()
                    {
                        Body = BinaryData.FromString("Produto Cadastrado com sucesso."),
                        SessionId = msg.ReplyToSessionId,
                        ApplicationProperties =
                    {
                        ["ReplyTo"] = msg.ReplyTo,
                        ["SessionId"] = msg.SessionId,
                        ["MessageId"] = msg.MessageId
                    }
                    }, stoppingToken);

                }
                else
                {
                    _logger.LogWarning($"Message {msg.MessageId} has no ReplyTo property.");
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
