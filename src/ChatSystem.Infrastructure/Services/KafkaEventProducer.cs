using System.Text.Json;
using ChatSystem.Application.Interfaces;
using ChatSystem.Infrastructure.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace ChatSystem.Infrastructure.Services;

/// <summary>
/// High-performance Kafka implementation of IEventProducer.
/// </summary>
public class KafkaEventProducer : IEventProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;

    /// <summary>
    /// Initializes a new instance of the <see cref="KafkaEventProducer"/> class.
    /// <param name="options">Kafka configuration options.</param>
    /// </summary>
    public KafkaEventProducer(IOptions<KafkaOptions> options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            // Goal: Ensure message order and no duplicates
            EnableIdempotence = true,
            Acks = Acks.All,
            MessageSendMaxRetries = 3
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    /// <summary>
    /// Publishes an event to a Kafka topic.
    /// </summary>
    public async Task PublishAsync(string topic, string key, object eventData)
    {
        var payload = JsonSerializer.Serialize(eventData);
        
        var message = new Message<string, string>
        {
            Key = key,
            Value = payload
        };

        await _producer.ProduceAsync(topic, message);
    }

    /// <summary>
    /// Flushes any pending messages and disposes the producer.
    /// </summary>
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
