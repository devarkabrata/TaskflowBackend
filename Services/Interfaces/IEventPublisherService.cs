namespace TaskFlowBackend.Services.Interfaces
{
    public interface IEventPublisherService
    {
        Task PublishAsync<TEvent>(string routingKey, TEvent payload, CancellationToken cancellationToken = default) where TEvent : class;
    }
}
