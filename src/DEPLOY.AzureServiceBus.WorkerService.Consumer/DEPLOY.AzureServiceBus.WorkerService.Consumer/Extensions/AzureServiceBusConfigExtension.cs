using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.WorkerService.Consumer.Config;
using Microsoft.Extensions.Options;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer.Extensions
{
    public static class AzureServiceBusConfigExtension
    {
        public static void AddAzureServiceBusConfig(this IServiceCollection services)
        {
            services.AddSingleton(_ =>
            {
                var config = _.GetRequiredService<IOptions<ParametersConfig>>().Value;
                return new ServiceBusClient(config.AzureServiceBus.ConnectionString, new ServiceBusClientOptions()
                {

                });
            });
        }
    }
}
