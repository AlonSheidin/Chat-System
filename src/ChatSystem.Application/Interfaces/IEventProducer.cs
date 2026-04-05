namespace ChatSystem.Application.Interfaces;

/// <summary>
/// Defines the contract for publishing events to the message bus.
/// </summary>
public interface IEventProducer
{
    /// <summary>
    /// Publishes an event to a specific topic with a partition key.
    /// </summary>
    Task PublishAsync(string topic, string key, object eventData);
}
