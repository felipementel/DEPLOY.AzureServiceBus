using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace DEPLOY.AzureServiceBus.Function.Consumer
{
    public class ServiceBusQueueBatchConsumer
    {
        private readonly ILogger<ServiceBusQueueBatchConsumer> _logger;
        const string _queueName = "simple-batch/$deadletterqueue";

        public ServiceBusQueueBatchConsumer(
            ILogger<ServiceBusQueueBatchConsumer> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ServiceBusQueueBatchConsumer))]
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
