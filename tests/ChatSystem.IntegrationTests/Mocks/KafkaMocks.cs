using ChatSystem.Application.Interfaces;

namespace ChatSystem.IntegrationTests.Mocks;

public class MockEventProducer : IEventProducer
{
    public Task PublishAsync(string topic, string key, object eventData)
    {
        // No-op for tests, or we could store in a list for verification
        return Task.CompletedTask;
    }
}

public class MockEventConsumer : IEventConsumer
{
    public Task ConsumeAsync(IEnumerable<string> topics, Func<string, string, Task> handler, CancellationToken cancellationToken, string? groupId = null)
    {
        // No-op for tests, doesn't start a real loop
        return Task.CompletedTask;
    }
}
