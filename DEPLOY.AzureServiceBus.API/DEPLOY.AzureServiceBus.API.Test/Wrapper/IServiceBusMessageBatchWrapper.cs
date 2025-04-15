using Azure.Messaging.ServiceBus;

public interface IServiceBusMessageBatchWrapper
{
    bool TryAddMessage(ServiceBusMessage message);
    int Count { get; }
    long SizeInBytes { get; }
    long MaxSizeInBytes { get; }
}