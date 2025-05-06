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
    public class QueueSessionTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly Mock<ServiceBusClient> _mockServiceBusClient;
        private readonly Mock<ServiceBusSender> _mockServiceBusSender;

        public QueueSessionTests()
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

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public async Task PostPartitionSessionQueue_ReturnsAccepted(int qtd)
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
            var response = await _httpClient.PostAsync($"api/v1/queue-partition-session/partition-session/{qtd}", null);

            // Assert
            //Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public async Task PostPartitionSessionQueueBatch_WithQtd_ReturnsAccepted(int qtd)
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
            var response = await _httpClient.PostAsync($"/api/v1/queue-partition-session/partition-session/batch/{qtd}", null);

            // Assert
            //Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            //_mockServiceBusSender
            //    .Verify(sender => sender.SendMessagesAsync(
            //        It.IsAny<ServiceBusMessageBatch>(),
            //        It.IsAny<CancellationToken>()),
            //    Times.Once);
        }
    }
}
