using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.API.Config;
using DEPLOY.AzureServiceBus.API.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DEPLOY.AzureServiceBus.API.Test.Extensions
{
    public class AzureServiceBusConfigExtensionTests
    {
        [Fact]
        public void AddAzureServiceBusConfig_ShouldRegisterServiceBusClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var mockConfig = new Mock<IOptions<ParametersConfig>>();
            ParametersConfig parametersConfig = new ParametersConfig();
            parametersConfig.AzureServiceBus = new Config.AzureServiceBus();
            parametersConfig.AzureServiceBus.ConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

            mockConfig.Setup(m => m.Value).Returns(parametersConfig);

            services.AddSingleton(mockConfig.Object);

            // Act
            services.AddAzureServiceBusConfig();
            var serviceProvider = services.BuildServiceProvider();
            var serviceBusClient = serviceProvider.GetService<ServiceBusClient>();

            // Assert
            Assert.NotNull(serviceBusClient);
            Assert.IsType<ServiceBusClient>(serviceBusClient);
        }
    }
}
