using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.API.Config;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Net;
using Testcontainers.ServiceBus;
using Xunit;

namespace DEPLOY.AzureServiceBus.API.Test
{
    public class QueueIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _httpClient;
        private readonly ServiceBusClient _ServiceBusClient;
        private readonly ServiceBusSender _ServiceBusSender;
        private readonly ServiceBusContainer _serviceBusContainer;

        public QueueIntegrationTests()
        {
            var configFile = Path.Combine(Directory.GetCurrentDirectory(), "Config.json");

            _serviceBusContainer = new ServiceBusBuilder()
            //#if RUN_LOCAL
            //   .WithDockerEndpoint("tcp://localhost:2375")
            //#endif
              .WithImage("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
              .WithAcceptLicenseAgreement(true)
              //.WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
              .WithBindMount(configFile, "/ServiceBus_Emulator/ConfigFiles/Config.json")
              .WithPortBinding(5672, 5672)
              .Build();

            ParametersConfig config = new ParametersConfig();
            config.AzureServiceBus = new Config.AzureServiceBus();
            config.AzureServiceBus.ConnectionString = "Endpoint=amqp://127.0.0.1:5672/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true";

            Console.Write(config.AzureServiceBus.ConnectionString);

            IOptions<ParametersConfig> MockOptions = Options.Create<ParametersConfig>(config);

            _ServiceBusClient = new ServiceBusClient(config.AzureServiceBus.ConnectionString);

            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(MockOptions);

                    services.AddScoped(_ =>
                    {
                        var config = _.GetRequiredService<IOptions<ParametersConfig>>().Value;
                        return new ServiceBusClient(config.AzureServiceBus!.ConnectionString, new ServiceBusClientOptions()
                        {

                        });
                    });
                });
            });

            _httpClient = _factory.CreateClient();
        }

        [Fact]
        public async Task PostSimpleQueue_ReturnsAccepted()
        {
            await _serviceBusContainer.StartAsync();

            //var string2 = _serviceBusContainer.GetConnectionString();
            
            // Arrange
            _ServiceBusClient.CreateSender("simple-product");

            // Act
            var response = await _httpClient.PostAsync("/api/v1/queue/simple", null);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }
    }
}
