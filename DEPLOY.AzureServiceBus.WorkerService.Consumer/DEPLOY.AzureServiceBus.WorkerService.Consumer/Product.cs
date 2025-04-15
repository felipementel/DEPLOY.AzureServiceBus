using System.Text.Json.Serialization;

namespace DEPLOY.AzureServiceBus.WorkerService.Consumer
{
    public class Product : BaseMessage<int>
    {
        public required string SKU { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required int Quantity { get; set; }
    }

    public class BaseMessage<TVersion>
    {
        public required TVersion Version { get; set; }
        public required string MessageType { get; set; }
    }

    public enum MessageType : byte
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        Command = 1,
        [JsonConverter(typeof(JsonStringEnumConverter))]
        Event = 2
    }
}
