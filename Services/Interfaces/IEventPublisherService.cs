namespace TaskFlowBackend.Services.Interfaces
{
    public interface IEventPublisherService
    {
        Task PublishAsync<TEvent>(string exchangeName, string routingKey, TEvent payload, CancellationToken cancellationToken = default) where TEvent : class;
    }
}
