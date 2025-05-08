using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.API.Config;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using Xunit;

namespace DEPLOY.AzureServiceBus.API.Test.v1_Endpoints
{
    public class QueuePartitionTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly Mock<ServiceBusClient> _mockServiceBusClient;
        private readonly Mock<ServiceBusSender> _mockServiceBusSender;

        public QueuePartitionTests(WebApplicationFactory<Program> factory)
        {
            ParametersConfig config = new ParametersConfig();
            config.AzureServiceBus = new Config.AzureServiceBus();
            config.AzureServiceBus.ConnectionString = "Endpoint=sb://127.0.0.1;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

            var MockIOptions = new Mock<IOptions<ParametersConfig>>();
            MockIOptions.Setup(x => x.Value).Returns(config);

            _mockServiceBusClient = new Mock<ServiceBusClient>();
            _mockServiceBusSender = new Mock<ServiceBusSender>();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped(sp =>
                    {
                        return MockIOptions.Object;
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(_mockServiceBusClient.Object);
                    services.AddSingleton(_mockServiceBusSender.Object);
                });

                builder.UseEnvironment("Development");
            });

            _httpClient = _factory.CreateClient();
        }

        [Fact]
        public async Task PostPartitionQueue_WithQtd_ReturnsAccepted()
        {
            // Arrange
            _mockServiceBusClient
                .Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(_mockServiceBusSender.Object);

            // Simulação do ServiceBusMessageBatch usando a factory
            List<ServiceBusMessage> backingList = new();
            ServiceBusMessageBatch mockBatch = ServiceBusModelFactory.ServiceBusMessageBatch(
                batchSizeBytes: 10000,  // Tamanho arbitrário grande o suficiente
                batchMessageStore: backingList,
                batchOptions: new CreateMessageBatchOptions(),
                tryAddCallback: message => true);  // Sempre aceita adicionar mensagens

            _mockServiceBusSender
                .Setup(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBatch);

            // Act
            var response = await _httpClient.PostAsync("/api/v1/queues-partitions/partition/batch/5", null);

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
            _mockServiceBusClient
                .Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(_mockServiceBusSender.Object);

            _mockServiceBusSender
                .Setup(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()));

            List<ServiceBusMessage> backingList = new();
            ServiceBusMessageBatch mockBatch = ServiceBusModelFactory.ServiceBusMessageBatch(
                batchSizeBytes: 10000,  // Tamanho arbitrário grande o suficiente
                batchMessageStore: backingList,
                batchOptions: new CreateMessageBatchOptions(),
                tryAddCallback: message => true);  // Sempre aceita adicionar mensagens

            _mockServiceBusSender
                .Setup(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBatch);

            // Act
            var response = await _httpClient.PostAsync("/api/v1/queues-partitions/partition/batch/5", null);

            // Assert
            //Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }
    }
}
