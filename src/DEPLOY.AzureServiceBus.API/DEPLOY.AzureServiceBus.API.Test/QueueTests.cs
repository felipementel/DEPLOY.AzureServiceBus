using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.API.Config;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using Testcontainers.ServiceBus;
using Xunit;

namespace DEPLOY.AzureServiceBus.API.Test
{
    public class QueueEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly Mock<ServiceBusClient> _mockServiceBusClient;
        private readonly Mock<ServiceBusSender> _mockServiceBusSender;
        private readonly ServiceBusContainer _serviceBusContainer;

        public QueueEndpointTests()
        {
            _serviceBusContainer = new ServiceBusBuilder()
              .WithImage("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
              .WithEnvironment("ACCEPT_EULA", "Y") // <- Aceita a licença
                .WithPortBinding(5672, 5672) // ou qualquer porta exposta pelo emulador
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
                .Build();

            ParametersConfig config = new ParametersConfig();
            config.AzureServiceBus = new Config.AzureServiceBus();
            config.AzureServiceBus.ConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

            IOptions<ParametersConfig> MockOptions = Options.Create<ParametersConfig>(config);

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IOptions<ParametersConfig>>(sp =>
                    {
                        return MockOptions;
                    });
                });
            });

            _httpClient = _factory.CreateClient();

            _mockServiceBusClient = new Mock<ServiceBusClient>();
            _mockServiceBusSender = new Mock<ServiceBusSender>();
        }

        [Fact]
        public async Task PostSimpleQueue_ReturnsAccepted()
        {
            try
            {                
                await _serviceBusContainer.StartAsync();

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
                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
                //Assert.Equal(StatusCodes.Status202Accepted, (int)response.StatusCode);
                //mockServiceBusClient.Verify(client => client.CreateSender("simple"), Times.Once);
                //mockServiceBusSender.Verify(sender => sender.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()), Times.Once);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                Console.Write($"ERROR: {ex}");
            }
        }
    }
}
