using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DEPLOY.AzureServiceBus.Function.Consumer.Topic
{
    public class ServiceBusTopicConsumer
    {
        private readonly ILogger<ServiceBusTopicConsumer> _logger;

        public ServiceBusTopicConsumer(ILogger<ServiceBusTopicConsumer> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ServiceBusTopicConsumer))]
        public async Task Run(
            [ServiceBusTrigger(
            topicName: "deploy-sem-particao",
            subscriptionName: "cliente-1",
            AutoCompleteMessages =false,
            Connection = "AzureServiceBus:Topic:ConnectionString",
            IsBatched = false,
            IsSessionsEnabled = false)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("**** TOPIC ****");
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
