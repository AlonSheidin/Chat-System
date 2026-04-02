using ChatSystem.Application.Interfaces;
using ChatSystem.Infrastructure.Options;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatSystem.Infrastructure.Services;

/// <summary>
/// High-performance Kafka implementation of IEventConsumer.
/// </summary>
public class KafkaEventConsumer : IEventConsumer
{
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaEventConsumer> _logger;

    public KafkaEventConsumer(IOptions<KafkaOptions> options, ILogger<KafkaEventConsumer> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Starts a long-running consumption loop.
    /// </summary>
    public async Task ConsumeAsync(IEnumerable<string> topics, Func<string, string, Task> handler, CancellationToken cancellationToken, string? groupId = null)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = groupId ?? _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false // We will commit manually after processing
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topics);

        _logger.LogInformation("Started Kafka consumer for topics: {Topics}", string.Join(", ", topics));

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(cancellationToken);
                    if (result?.Message == null) continue;

                    _logger.LogDebug("Received event from {Topic}: {Key}", result.Topic, result.Message.Key);

                    // Execute handler
                    await handler(result.Message.Key, result.Message.Value);

                    // Commit offset only after successful processing
                    consumer.Commit(result);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming Kafka message");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Kafka consumer loop");
                    // Add a small delay to avoid tight loop on persistent errors
                    await Task.Delay(1000, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Kafka consumer stopping...");
        }
        finally
        {
            consumer.Close();
        }
    }
}
