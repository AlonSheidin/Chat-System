using ChatSystem.Infrastructure.Options;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatSystem.Infrastructure.Services;

/// <summary>
/// Service responsible for ensuring that all required Kafka topics exist before the app starts.
/// </summary>
public interface IKafkaTopicInitializer
{
    Task EnsureTopicsCreatedAsync();
}

public class KafkaTopicInitializer : IKafkaTopicInitializer
{
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaTopicInitializer> _logger;

    public KafkaTopicInitializer(IOptions<KafkaOptions> options, ILogger<KafkaTopicInitializer> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnsureTopicsCreatedAsync()
    {
        var config = new AdminClientConfig { BootstrapServers = _options.BootstrapServers };
        using var adminClient = new AdminClientBuilder(config).Build();

        var topics = new[] { "message.send", "message.stored", "user.presence", "typing.events" };
        
        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
            var existingTopics = metadata.Topics.Select(t => t.Topic).ToList();

            var topicsToCreate = topics.Where(t => !existingTopics.Contains(t))
                                       .Select(t => new TopicSpecification { Name = t, NumPartitions = 3, ReplicationFactor = 1 })
                                       .ToList();

            if (topicsToCreate.Any())
            {
                _logger.LogInformation("Creating missing Kafka topics: {Topics}", string.Join(", ", topicsToCreate.Select(t => t.Name)));
                await adminClient.CreateTopicsAsync(topicsToCreate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to ensure Kafka topics are created. The app will rely on auto-creation if enabled.");
        }
    }
}
