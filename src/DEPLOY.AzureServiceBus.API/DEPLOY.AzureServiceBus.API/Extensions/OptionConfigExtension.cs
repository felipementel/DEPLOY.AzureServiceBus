using DEPLOY.AzureServiceBus.API.Config;

namespace DEPLOY.AzureServiceBus.API.Extensions
{
    public static class OptionConfigExtension
    {
        public static void AddOptionConfig(this IServiceCollection services)
        {
            services
                .AddOptionsWithValidateOnStart<ParametersConfig>()
                .BindConfiguration("ParametersConfig")
                .ValidateDataAnnotations()
                .Validate(config =>
                {
                    if (config is null || config.AzureServiceBus is null || config.AzureServiceBus.ConnectionString is null)
                    {
                        throw new Exception("Azure Service Bus is not configured");
                    }
                    return true;
                });
        }
    }
}
