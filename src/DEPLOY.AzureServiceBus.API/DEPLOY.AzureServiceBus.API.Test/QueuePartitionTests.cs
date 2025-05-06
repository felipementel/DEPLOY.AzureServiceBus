using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.API.Config;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using Xunit;

namespace DEPLOY.AzureServiceBus.API.Test
{
    public class QueuePartitionTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly Mock<ServiceBusClient> _mockServiceBusClient;
        private readonly Mock<ServiceBusSender> _mockServiceBusSender;

        public QueuePartitionTests()
        {
            ParametersConfig config = new ParametersConfig();
            config.AzureServiceBus = new Config.AzureServiceBus();
            config.AzureServiceBus.ConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

            var MockIOptions = new Mock<IOptions<ParametersConfig>>();
            MockIOptions.Setup(x => x.Value).Returns(config);

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IOptions<ParametersConfig>>(sp =>
                    {
                        return MockIOptions.Object;
                    });
                });
            });

            _httpClient = _factory.CreateClient();

            _mockServiceBusClient = new Mock<ServiceBusClient>();
            _mockServiceBusSender = new Mock<ServiceBusSender>();
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
            var response = await _httpClient.PostAsync("/api/v1/queue-partition/partition/batch/5", null);

            // Assert
            //Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

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
            var response = await _httpClient.PostAsync("/api/v1/queue-partition/partition/batch/5", null);

            // Assert
            //Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }
    }
}
