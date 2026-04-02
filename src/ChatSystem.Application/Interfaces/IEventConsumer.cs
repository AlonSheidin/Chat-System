namespace ChatSystem.Application.Interfaces;

/// <summary>
/// Defines the contract for consuming events from the message bus.
/// </summary>
public interface IEventConsumer
{
    /// <summary>
    /// Starts consuming events from specified topics and executes a handler for each message.
    /// </summary>
    /// <param name="topics">The list of Kafka topics to subscribe to.</param>
    /// <param name="handler">The asynchronous handler for processing each event.</param>
    /// <param name="cancellationToken">Token to stop consumption.</param>
    /// <param name="groupId">Optional override for the consumer group ID.</param>
    Task ConsumeAsync(IEnumerable<string> topics, Func<string, string, Task> handler, CancellationToken cancellationToken, string? groupId = null);
}
