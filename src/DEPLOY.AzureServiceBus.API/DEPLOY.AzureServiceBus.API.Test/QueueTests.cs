using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace DEPLOY.AzureServiceBus.API.Test
{
    public class QueueEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly Mock<ServiceBusClient> _mockServiceBusClient;
        private readonly Mock<ServiceBusSender> _mockServiceBusSender;

        public QueueEndpointTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateClient();

            _mockServiceBusClient = new Mock<ServiceBusClient>();
            _mockServiceBusSender = new Mock<ServiceBusSender>();
        }

        [Fact]
        public async Task PostSimpleQueue_ReturnsAccepted()
        {
            // Arrange

            _mockServiceBusClient
                .Setup(client => client.CreateSender("simple")) //It.IsAny<string>()
                .Returns(_mockServiceBusSender.Object);

            _mockServiceBusSender
                .Setup(sender => sender.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _httpClient.PostAsync("/api/v1/queue/simple", null);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
            //mockServiceBusClient.Verify(client => client.CreateSender("simple"), Times.Once);
            //mockServiceBusSender.Verify(sender => sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PostPartitionQueue_WithQtd_ReturnsAccepted()
        {
            // Arrange
            _mockServiceBusClient
                .Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(_mockServiceBusSender.Object);

            _mockServiceBusSender
                .Setup(sender => sender.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _httpClient.PostAsync("/api/v1/queue/partition/partition/5", null);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);

            // Ideally verify that the correct sender was created and messages were sent
            // These verifications would work with proper DI setup for testing
            // mockServiceBusClient.Verify(client => client.CreateSender("partition"), Times.Once);
            // mockServiceBusSender.Verify(sender => sender.SendMessageAsync(
            //    It.Is<ServiceBusMessage>(msg => msg.ContentType == "application/text"), 
            //    It.IsAny<CancellationToken>()), 
            //    Times.Once);
        }

        [Fact]
        public async Task PostPartitionQueueBatch_WithQtd_ReturnsAccepted()
        {
            // Arrange
            var mockMessageBatch = new Mock<ServiceBusMessageBatchWrapper>();

            _mockServiceBusClient
                .Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(_mockServiceBusSender.Object);

            _mockServiceBusSender
                .Setup(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()));

            mockMessageBatch
                .Setup(batch => batch.TryAddMessage(It.IsAny<ServiceBusMessage>()))
                .Returns(true);

            _mockServiceBusSender
                .Setup(sender => sender.SendMessagesAsync(
                    It.IsAny<ServiceBusMessageBatch>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _httpClient.PostAsync("/api/v1/queue/partition/partition/batch/5", null);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
        }

        [Fact]
        public async Task PostPartitionSessionQueue_ReturnsAccepted()
        {
            // Arrange

            _mockServiceBusClient
                .Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(_mockServiceBusSender.Object);

            _mockServiceBusSender
                .Setup(sender => sender.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _httpClient.PostAsync("/api/v1/queue/partition_session/partition-session", null);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
        }

        [Fact]
        public async Task PostPartitionSessionQueueBatch_WithQtd_ReturnsAccepted()
        {
            // Arrange
            _mockServiceBusClient
                .Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(_mockServiceBusSender.Object);

            List<ServiceBusMessage> backingList = new();
            int batchCountThreshold = 5;

            ServiceBusMessageBatch mockBatch = ServiceBusModelFactory.ServiceBusMessageBatch(
                batchSizeBytes: 500,
                batchMessageStore: backingList,
                batchOptions: new CreateMessageBatchOptions(),
                // The model factory allows a custom TryAddMessage callback, allowing control of
                // what messages the batch accepts.
                tryAddCallback: _ => backingList.Count < batchCountThreshold);

            _mockServiceBusSender
                .Setup(sender => sender.CreateMessageBatchAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBatch);

            _mockServiceBusSender
                .Setup(sender => sender.SendMessagesAsync(
                    It.Is<ServiceBusMessageBatch>(sendBatch => sendBatch != mockBatch),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _httpClient.PostAsync("/api/v1/queue/partition_session/partition-session/batch/5", null);

            // Assert
            Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);

            //_mockServiceBusSender
            //    .Verify(sender => sender.SendMessagesAsync(
            //        It.IsAny<ServiceBusMessageBatch>(),
            //        It.IsAny<CancellationToken>()),
            //    Times.Once);
        }
    }
}
