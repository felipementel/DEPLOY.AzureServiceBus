using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.API.Config;
using DEPLOY.AzureServiceBus.API.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;
using static DEPLOY.AzureServiceBus.API.Util.GenerateData;

namespace DEPLOY.AzureServiceBus.API.Test.v1_Endpoints
{
    public class QueueEndpointTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly Mock<ServiceBusClient> _mockServiceBusClient;
        private readonly Mock<ServiceBusSender> _mockServiceBusSender;

        public QueueEndpointTests(WebApplicationFactory<Program> factory)
        {
            // Configuração básica
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
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            _mockServiceBusClient.Verify(client => client.CreateSender("simple-product"), Times.Once);
            _mockServiceBusSender.Verify(sender => sender.SendMessageAsync(
                It.Is<ServiceBusMessage>(msg => msg.ContentType == "application/json"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PostSimpleDuplicateQueue_WithMessage_ReturnsAccepted()
        {
            // Arrange
            string testMessage = "Test Message";
            _mockServiceBusClient
                .Setup(client => client.CreateSender("simple-duplicate"))
                .Returns(_mockServiceBusSender.Object);

            _mockServiceBusSender
                .Setup(sender => sender.SendMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var json = JsonSerializer.Serialize(testMessage);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/api/v1/queue/simple-duplicate", content);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            _mockServiceBusClient.Verify(client => client.CreateSender("simple-duplicate"), Times.Once);
            _mockServiceBusSender.Verify(sender => sender.SendMessageAsync(
                It.Is<ServiceBusMessage>(msg => 
                    msg.ContentType == "application/json" && 
                    msg.MessageId == testMessage.Length.ToString()),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PostSimpleScheduleQueue_WithMessageAndHeader_ReturnsAccepted()
        {
            // Arrange
            string testMessage = "Scheduled Message";
            double scheduleInSeconds = 30;

            _mockServiceBusClient
                .Setup(client => client.CreateSender("simple-schedule"))
                .Returns(_mockServiceBusSender.Object);

            _mockServiceBusSender
                .Setup(sender => sender.ScheduleMessageAsync(
                    It.IsAny<ServiceBusMessage>(),
                    It.IsAny<DateTimeOffset>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(1L); // Retorna um long que representa o ID da mensagem agendada

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/queue/simple-schedule");
            request.Headers.Add("scheduleInSecconds", scheduleInSeconds.ToString());
            var json = JsonSerializer.Serialize(testMessage);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            _mockServiceBusClient.Verify(client => client.CreateSender("simple-schedule"), Times.Once);
            _mockServiceBusSender.Verify(sender => sender.ScheduleMessageAsync(
                It.Is<ServiceBusMessage>(msg => 
                    msg.ContentType == "application/json" && 
                    msg.Body.ToString() == testMessage),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(10)]
        public async Task PostSimpleBatchQueue_WithQtdParameter_ReturnsAccepted(int qtd)
        {
            // Arrange
            _mockServiceBusClient
                .Setup(client => client.CreateSender("simple-batch"))
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

            _mockServiceBusSender
                .Setup(sender => sender.SendMessagesAsync(
                    It.IsAny<ServiceBusMessageBatch>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var response = await _httpClient.PostAsync($"/api/v1/queue/simple/{qtd}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            _mockServiceBusClient.Verify(client => client.CreateSender("simple-batch"), Times.Once);
            _mockServiceBusSender.Verify(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _mockServiceBusSender.Verify(sender => sender.SendMessagesAsync(
                It.IsAny<ServiceBusMessageBatch>(),
                It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SendBatchAsync_WithMessagesThatFitInBatch_SendsOnceSuccessfully()
        {
            // Arrange
            var messages = new List<ServiceBusMessage>
            {
                new ServiceBusMessage { Body = BinaryData.FromString("Message 1") },
                new ServiceBusMessage { Body = BinaryData.FromString("Message 2") }
            };

            List<ServiceBusMessage> backingList = new();
            ServiceBusMessageBatch mockBatch = ServiceBusModelFactory.ServiceBusMessageBatch(
                batchSizeBytes: 10000,
                batchMessageStore: backingList,
                batchOptions: new CreateMessageBatchOptions(),
                tryAddCallback: message => true);  // Todas as mensagens cabem no lote

            _mockServiceBusSender
                .Setup(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBatch);

            _mockServiceBusSender
                .Setup(sender => sender.SendMessagesAsync(
                    It.IsAny<ServiceBusMessageBatch>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await Endpoints.v1.QueueEndpoint.SendBatchAsync(_mockServiceBusSender.Object, messages);

            // Assert
            _mockServiceBusSender.Verify(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockServiceBusSender.Verify(sender => sender.SendMessagesAsync(
                It.IsAny<ServiceBusMessageBatch>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendBatchAsync_WithMessagesThatExceedBatchSize_CreatesMultipleBatches()
        {
            // Arrange
            var messages = new List<ServiceBusMessage>
            {
                new ServiceBusMessage { Body = BinaryData.FromString("Message 1") },
                new ServiceBusMessage { Body = BinaryData.FromString("Message 2") },
                new ServiceBusMessage { Body = BinaryData.FromString("Message 3") }
            };

            int messageCount = 0;
            
            // Configuração para simular que apenas a primeira mensagem cabe no primeiro lote
            List<ServiceBusMessage> backingList1 = new();
            ServiceBusMessageBatch mockBatch1 = ServiceBusModelFactory.ServiceBusMessageBatch(
                batchSizeBytes: 1000,
                batchMessageStore: backingList1,
                batchOptions: new CreateMessageBatchOptions(),
                tryAddCallback: _ => messageCount++ < 1);  // Apenas a primeira mensagem cabe

            // Configuração para o segundo lote que aceitará o resto das mensagens
            List<ServiceBusMessage> backingList2 = new();
            ServiceBusMessageBatch mockBatch2 = ServiceBusModelFactory.ServiceBusMessageBatch(
                batchSizeBytes: 1000,
                batchMessageStore: backingList2,
                batchOptions: new CreateMessageBatchOptions(),
                tryAddCallback: _ => true);  // Todas as mensagens restantes cabem

            _mockServiceBusSender
                .SetupSequence(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBatch1)
                .ReturnsAsync(mockBatch2);

            _mockServiceBusSender
                .Setup(sender => sender.SendMessagesAsync(
                    It.IsAny<ServiceBusMessageBatch>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await Endpoints.v1.QueueEndpoint.SendBatchAsync(_mockServiceBusSender.Object, messages);

            // Assert
            _mockServiceBusSender.Verify(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
            _mockServiceBusSender.Verify(sender => sender.SendMessagesAsync(
                It.IsAny<ServiceBusMessageBatch>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task SendBatchAsync_WithServiceBusException_HandlesExceptionGracefully()
        {
            // Arrange
            var messages = new List<ServiceBusMessage>
            {
                new ServiceBusMessage { Body = BinaryData.FromString("Message 1") }
            };

            List<ServiceBusMessage> backingList = new();
            ServiceBusMessageBatch mockBatch = ServiceBusModelFactory.ServiceBusMessageBatch(
                batchSizeBytes: 1000,
                batchMessageStore: backingList,
                batchOptions: new CreateMessageBatchOptions(),
                tryAddCallback: _ => true);  // A mensagem cabe no lote

            _mockServiceBusSender
                .Setup(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockBatch);

            _mockServiceBusSender
                .Setup(sender => sender.SendMessagesAsync(
                    It.IsAny<ServiceBusMessageBatch>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ServiceBusException("Test exception", ServiceBusFailureReason.ServiceBusy));

            // Redirecionando a saída do console para capturar mensagens de log
            var originalConsoleOut = Console.Out;
            using var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            // Act - Não deve lançar exceção para o chamador
            await Endpoints.v1.QueueEndpoint.SendBatchAsync(_mockServiceBusSender.Object, messages);
            
            // Restaura a saída do console
            Console.SetOut(originalConsoleOut);

            // Assert
            _mockServiceBusSender.Verify(sender => sender.CreateMessageBatchAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockServiceBusSender.Verify(sender => sender.SendMessagesAsync(
                It.IsAny<ServiceBusMessageBatch>(),
                It.IsAny<CancellationToken>()), Times.Once);
            
            // Verifica se a mensagem de erro foi registrada no console
            Assert.Contains("Error ServiceBusException sending batch:", consoleOutput.ToString());
        }
    }
}
