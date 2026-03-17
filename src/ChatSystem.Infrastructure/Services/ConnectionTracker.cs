using System.Collections.Concurrent;

namespace ChatSystem.Infrastructure.Services;

public interface IConnectionTracker
{
    Task AddConnection(Guid userId, string connectionId);
    Task RemoveConnection(Guid userId, string connectionId);
    Task<IEnumerable<string>> GetConnections(Guid userId);
    Task<IEnumerable<Guid>> GetOnlineUsers();
    Task<bool> IsUserOnline(Guid userId);
}

public class ConnectionTracker : IConnectionTracker
{
    // UserId -> Set of ConnectionIds (to support multiple devices)
    private static readonly ConcurrentDictionary<Guid, HashSet<string>> OnlineUsers = new();

    public Task AddConnection(Guid userId, string connectionId)
    {
        OnlineUsers.AddOrUpdate(userId, 
            _ => new HashSet<string> { connectionId }, 
            (_, connections) => 
            {
                lock (connections)
                {
                    connections.Add(connectionId);
                }
                return connections;
            });
        
        return Task.CompletedTask;
    }

    public Task RemoveConnection(Guid userId, string connectionId)
    {
        if (OnlineUsers.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    OnlineUsers.TryRemove(userId, out _);
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetConnections(Guid userId)
    {
        if (OnlineUsers.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return Task.FromResult<IEnumerable<string>>(connections.ToList());
            }
        }
        return Task.FromResult<IEnumerable<string>>(Enumerable.Empty<string>());
    }

    public Task<IEnumerable<Guid>> GetOnlineUsers()
    {
        return Task.FromResult<IEnumerable<Guid>>(OnlineUsers.Keys.ToList());
    }

    public Task<bool> IsUserOnline(Guid userId)
    {
        return Task.FromResult(OnlineUsers.ContainsKey(userId));
    }
}
