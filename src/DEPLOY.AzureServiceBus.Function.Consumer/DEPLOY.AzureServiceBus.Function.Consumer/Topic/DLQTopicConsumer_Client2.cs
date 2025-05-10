using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DEPLOY.AzureServiceBus.Function.Consumer.Topic
{
    public class DLQTopicConsumer_Client2
    {
        private readonly ILogger<DLQTopicConsumer_Client2> _logger;
        const string _topicName = "cloud-events/$deadletterqueue";
        const string _subscriberName = "cloud-events-subs-2";

        public DLQTopicConsumer_Client2(ILogger<DLQTopicConsumer_Client2> logger)
        {
            _logger = logger;
        }

        [Function("DLQTopicConsumerCloud-events-subs-2")]
        public async Task Run(
            [ServiceBusTrigger(
            topicName: _topicName,
            subscriptionName: _subscriberName,
            AutoCompleteMessages =false,
            Connection = "AzureServiceBus:Topic:ConnectionString",
            IsBatched = false,
            IsSessionsEnabled = false)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("**** TOPIC ****");
            _logger.LogInformation($"SubscriberName: {_subscriberName}");
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
