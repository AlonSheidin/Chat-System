namespace ChatSystem.Application.Interfaces;

/// <summary>
/// Fast in-memory ingress point for events. 
/// Decouples the caller from Kafka network latency.
/// </summary>
public interface IBackgroundEventPublisher
{
    /// <summary>
    /// Queues an event for background publishing to Kafka.
    /// </summary>
    bool Publish(string topic, string key, object eventData);
}
