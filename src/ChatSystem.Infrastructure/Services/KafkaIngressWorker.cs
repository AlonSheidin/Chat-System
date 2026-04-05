using System.Threading.Channels;
using ChatSystem.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatSystem.Infrastructure.Services;

/// <summary>
/// Managed background worker that buffers incoming events in RAM 
/// and drains them into Kafka as fast as the network allows.
/// </summary>
public class KafkaIngressWorker : BackgroundService, IBackgroundEventPublisher
{
    private readonly IEventProducer _producer;
    private readonly ILogger<KafkaIngressWorker> _logger;
    private readonly Channel<IngressEvent> _channel;

    public KafkaIngressWorker(IEventProducer producer, ILogger<KafkaIngressWorker> logger)
    {
        _producer = producer;
        _logger = logger;
        
        // Bounded channel to prevent RAM exhaustion if Kafka is down.
        // 10,000 events buffer is plenty for a single instance.
        _channel = Channel.CreateBounded<IngressEvent>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait // Wait for space, or use DropWrite if loss is preferred
        });
    }

    /// <summary>
    /// Fast non-blocking publish to the internal RAM channel.
    /// </summary>
    public bool Publish(string topic, string key, object eventData)
    {
        var result = _channel.Writer.TryWrite(new IngressEvent(topic, key, eventData));
        if (!result)
        {
            _logger.LogWarning("Ingress buffer full! Dropping event for topic {Topic}", topic);
        }
        return result;
    }

    /// <summary>
    /// Long-running task that drains the RAM channel and pushes to Kafka.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka Ingress Worker started.");

        await foreach (var @event in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _producer.PublishAsync(@event.Topic, @event.Key, @event.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to drain event to Kafka for topic {Topic}", @event.Topic);
            }
        }
    }

    private record IngressEvent(string Topic, string Key, object Data);
}
