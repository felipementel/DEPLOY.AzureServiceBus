using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DEPLOY.AzureServiceBus.Function.Consumer.Queue
{
    public class DLQQueuePartitionConsumer
    {
        private readonly ILogger<DLQQueuePartitionConsumer> _logger;
        const string _queueName = "partition/$deadletterqueue";

        public DLQQueuePartitionConsumer(
            ILogger<DLQQueuePartitionConsumer> logger)
        {
            _logger = logger;
        }

        [Function(nameof(DLQQueuePartitionConsumer))]
        public async Task Run(
            [ServiceBusTrigger(
            queueName: _queueName,
            AutoCompleteMessages = false,
            Connection = "AzureServiceBus:Queue:ConnectionString",
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
                _logger.LogInformation("*** DLQ MESSAGE DETAIL ***");
                _logger.LogInformation($"   DeadLetterSource: {message.DeadLetterSource}");
                _logger.LogInformation($"   DeadLetterReason: {message.DeadLetterReason}");
                _logger.LogInformation($"   DeadLetterErrorDescription: {message.DeadLetterErrorDescription}");

                await messageActions.CompleteMessageAsync(message);
            }
        }
    }
}
