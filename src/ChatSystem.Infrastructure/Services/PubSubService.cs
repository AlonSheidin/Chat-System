using System.Text.Json;
using StackExchange.Redis;

namespace ChatSystem.Infrastructure.Services;

/// <summary>
/// Service for cross-instance communication using Redis Pub/Sub.
/// </summary>
public interface IPubSubService
{
    /// <summary>
    /// Publishes a message to a specific Redis channel.
    /// </summary>
    Task PublishAsync<T>(string channel, T message);

    /// <summary>
    /// Subscribes to a specific Redis channel and provides a handler for received messages.
    /// </summary>
    Task SubscribeAsync<T>(string channel, Action<T> handler);
}

public class RedisPubSubService : IPubSubService
{
    private readonly ISubscriber _subscriber;

    public RedisPubSubService(IConnectionMultiplexer multiplexer)
    {
        _subscriber = multiplexer.GetSubscriber();
    }

    public async Task PublishAsync<T>(string channel, T message)
    {
        var payload = JsonSerializer.Serialize(message);
        await _subscriber.PublishAsync(RedisChannel.Literal(channel), payload);
    }

    public async Task SubscribeAsync<T>(string channel, Action<T> handler)
    {
        await _subscriber.SubscribeAsync(RedisChannel.Literal(channel), (redisChannel, value) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<T>(value.ToString());
                if (message != null)
                {
                    handler(message);
                }
            }
            catch
            {
                // In a production app, log the deserialization error
            }
        });
    }
}
