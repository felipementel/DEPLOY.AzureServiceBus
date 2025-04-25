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
                .Setup(client => client.CreateSender("simple-product"))
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
    }
}
