namespace ChatSystem.Infrastructure.Options;

/// <summary>
/// Configuration options for Apache Kafka.
/// </summary>
public class KafkaOptions
{
    public const string SectionName = "Kafka";

    /// <summary>
    /// Gets or sets the list of broker addresses (e.g., "127.0.0.1:9092").
    /// </summary>
    public string BootstrapServers { get; set; } = "127.0.0.1:9092";

    /// <summary>
    /// Gets or sets the default consumer group ID.
    /// </summary>
    public string GroupId { get; set; } = "chat-system-group";
}
