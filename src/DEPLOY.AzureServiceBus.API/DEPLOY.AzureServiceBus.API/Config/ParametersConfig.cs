namespace DEPLOY.AzureServiceBus.API.Config
{
    public class ParametersConfig
    {
        public AzureServiceBus? AzureServiceBus { get; set; }
    }

    public class AzureServiceBus
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
