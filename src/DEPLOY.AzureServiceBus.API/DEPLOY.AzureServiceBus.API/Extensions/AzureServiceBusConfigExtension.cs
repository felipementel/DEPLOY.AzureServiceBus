using Azure.Messaging.ServiceBus;
using DEPLOY.AzureServiceBus.API.Config;
using Microsoft.Extensions.Options;

namespace DEPLOY.AzureServiceBus.API.Extensions
{
    public static class AzureServiceBusConfigExtension
    {
        public static void AddAzureServiceBusConfig(this IServiceCollection services)
        {
            services.AddScoped(_ =>
            {
                var config = _.GetRequiredService<IOptions<ParametersConfig>>().Value;
                return new ServiceBusClient(config.AzureServiceBus.ConnectionString, new ServiceBusClientOptions()
                {

                });
            });
        }
    }
}
