using System.Text.Json;
using RabbitMQ.Client;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class EventPublisherService : IEventPublisherService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IConnection _connection;
        private readonly ILogger<EventPublisherService> _logger;

        public EventPublisherService(IConnection connection, ILogger<EventPublisherService> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task PublishAsync<TEvent>(string exchangeName, string routingKey, TEvent payload, CancellationToken cancellationToken = default) where TEvent : class
        {
            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            var body = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonOptions);

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Published event {EventType} to exchange {Exchange} with routing key {RoutingKey}", typeof(TEvent).Name, exchangeName, routingKey);
        }
    }
}
