using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DEPLOY.AzureServiceBus.Function.Consumer.Queue
{
    public class DLQQueueSimpleReplyConsumer
    {
        private readonly ILogger<DLQQueueSimpleReplyConsumer> _logger;
        const string _queueName = "simple-reply/$deadletterqueue";

        public DLQQueueSimpleReplyConsumer(
            ILogger<DLQQueueSimpleReplyConsumer> logger)
        {
            _logger = logger;
        }

        [Function(nameof(DLQQueueSimpleReplyConsumer))]
        public async Task Run(
            [ServiceBusTrigger(
            queueName: _queueName,
            AutoCompleteMessages = false,
            Connection = "AzureServiceBus:Queue:ConnectionString",
            IsBatched = false,
            IsSessionsEnabled = false)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation($"**** QUEUE {_queueName} ****");
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
            _logger.LogInformation("*** DLQ MESSAGE DETAIL ***");
            _logger.LogInformation($"   DeadLetterSource: {message.DeadLetterSource}");
            _logger.LogInformation($"   DeadLetterReason: {message.DeadLetterReason}");
            _logger.LogInformation($"   DeadLetterErrorDescription: {message.DeadLetterErrorDescription}");

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
