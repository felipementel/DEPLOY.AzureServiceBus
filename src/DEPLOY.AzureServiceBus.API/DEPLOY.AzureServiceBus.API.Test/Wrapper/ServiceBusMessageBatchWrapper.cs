using Azure.Messaging.ServiceBus;

public class ServiceBusMessageBatchWrapper : IServiceBusMessageBatchWrapper
{
    private readonly ServiceBusMessageBatch _inner;

    public ServiceBusMessageBatchWrapper(ServiceBusMessageBatch inner)
    {
        _inner = inner;
    }

    public virtual bool TryAddMessage(ServiceBusMessage message) => _inner.TryAddMessage(message);
    public virtual int Count => _inner.Count;
    public virtual long SizeInBytes => _inner.SizeInBytes;
    public virtual long MaxSizeInBytes => _inner.MaxSizeInBytes;
}
