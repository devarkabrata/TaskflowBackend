using System.Text.Json;
using RabbitMQ.Client;
using TaskFlowBackend.Services.Interfaces;

namespace TaskFlowBackend.Services
{
    public class EventPublisherService : IEventPublisherService, IAsyncDisposable
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IConnection _connection;
        private readonly ILogger<EventPublisherService> _logger;
        private readonly string _exchangeName;
        private readonly SemaphoreSlim _channelLock = new(1, 1);
        private IChannel? _channel;

        public EventPublisherService(IConnection connection, ILogger<EventPublisherService> logger, IConfiguration configuration)
        {
            _connection = connection;
            _logger = logger;
            _exchangeName = configuration["RabbitMq:ExchangeName"] ?? "";
        }

        public async Task PublishAsync<TEvent>(string routingKey, TEvent payload, CancellationToken cancellationToken = default) where TEvent : class
        {
            try
            {
                var body = JsonSerializer.SerializeToUtf8Bytes(payload, _jsonOptions);

                var properties = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent
                };

                await _channelLock.WaitAsync(cancellationToken);
                try
                {
                    var channel = await GetOrOpenChannelAsync(cancellationToken);

                    await channel.BasicPublishAsync(
                        exchange: _exchangeName,
                        routingKey: routingKey,
                        mandatory: false,
                        basicProperties: properties,
                        body: body,
                        cancellationToken: cancellationToken);
                }
                finally
                {
                    _channelLock.Release();
                }

                _logger.LogInformation("Published event {EventType} to exchange {Exchange} with routing key {RoutingKey}", typeof(TEvent).Name, _exchangeName, routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event {EventType} to exchange {Exchange} with routing key {RoutingKey}", typeof(TEvent).Name, _exchangeName, routingKey);
            }
        }

        // Caller must hold _channelLock before invoking this.
        private async Task<IChannel> GetOrOpenChannelAsync(CancellationToken cancellationToken)
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }

            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            return _channel;
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
            }

            _channelLock.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
