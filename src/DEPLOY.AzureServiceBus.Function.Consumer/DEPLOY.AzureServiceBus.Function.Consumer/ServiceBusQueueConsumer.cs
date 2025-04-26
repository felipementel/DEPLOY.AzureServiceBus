using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DEPLOY.AzureServiceBus.Function.Consumer
{
    public class ServiceBusQueueConsumer
    {
        private readonly ILogger<ServiceBusQueueConsumer> _logger;

        public ServiceBusQueueConsumer(ILogger<ServiceBusQueueConsumer> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ServiceBusQueueConsumer))]
        public async Task Run(
            [ServiceBusTrigger(
            queueName: "simple-product/$deadletterqueue",
            AutoCompleteMessages = false,
            Connection = "AzureServiceBus:Queue:Conn1",
            IsBatched = false,
            IsSessionsEnabled = false)]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("**** QUEUE ****");
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
