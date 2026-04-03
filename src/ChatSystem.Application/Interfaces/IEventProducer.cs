namespace ChatSystem.Application.Interfaces;

/// <summary>
/// Defines the contract for publishing events to the message bus.
/// </summary>
public interface IEventProducer
{
    /// <summary>
    /// Publishes an event to a specific topic with a partition key.
    /// </summary>
    /// <typeparam name="T">The type of the event payload.</typeparam>
    /// <param name="topic">The Kafka topic name.</param>
    /// <param name="key">The partition key (e.g., chatId or userId) to ensure ordering.</param>
    /// <param name="eventData">The serializable payload.</param>
    Task PublishAsync<T>(string topic, string key, T eventData) where T : class;
}
