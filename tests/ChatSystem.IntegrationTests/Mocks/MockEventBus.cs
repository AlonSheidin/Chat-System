using ChatSystem.Application.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json;

namespace ChatSystem.IntegrationTests.Mocks;

public class MockEventBus : IEventProducer, IEventConsumer
{
    private readonly ConcurrentDictionary<string, List<Func<string, string, Task>>> _handlers = new();

    // IEventProducer
    public async Task PublishAsync(string topic, string key, object eventData)
    {
        var json = JsonSerializer.Serialize(eventData);
        if (_handlers.TryGetValue(topic, out var topicHandlers))
        {
            foreach (var handler in topicHandlers)
            {
                await handler(key, json);
            }
        }
    }

    // IEventConsumer
    public Task ConsumeAsync(IEnumerable<string> topics, Func<string, string, Task> handler, CancellationToken cancellationToken, string? groupId = null)
    {
        foreach (var topic in topics)
        {
            _handlers.AddOrUpdate(topic, 
                _ => new List<Func<string, string, Task>> { handler },
                (_, list) => { list.Add(handler); return list; });
        }
        return Task.CompletedTask;
    }
}
