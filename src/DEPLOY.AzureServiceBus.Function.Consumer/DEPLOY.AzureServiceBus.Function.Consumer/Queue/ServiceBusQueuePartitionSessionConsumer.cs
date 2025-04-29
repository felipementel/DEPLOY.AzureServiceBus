using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DEPLOY.AzureServiceBus.Function.Consumer.Queue
{
    public class ServiceBusQueuePartitionSessionConsumer
    {
        private readonly ILogger<ServiceBusQueuePartitionSessionConsumer> _logger;
        const string _queueName = "partition-session/$deadletterqueue";

        public ServiceBusQueuePartitionSessionConsumer(
            ILogger<ServiceBusQueuePartitionSessionConsumer> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ServiceBusQueuePartitionSessionConsumer))]
        public async Task Run(
            [ServiceBusTrigger(
            queueName: _queueName,
            AutoCompleteMessages = false,
            Connection = "AzureServiceBus:Queue:Conn1",
            IsBatched = true,
            IsSessionsEnabled = false)]
            ServiceBusReceivedMessage[] messages,
            ServiceBusMessageActions messageActions)
        {
            foreach (var message in messages)
            {
                _logger.LogInformation($"**** QUEUE {_queueName} ****");
                _logger.LogInformation("Message ID: {id}", message.MessageId);
                _logger.LogInformation("Message Body: {body}", message.Body);
                _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

                await messageActions.CompleteMessageAsync(message);
            }
        }
    }
}
