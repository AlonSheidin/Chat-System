namespace ChatSystem.Infrastructure.Services;

public enum UserStatus
{
    Offline,
    Online
}

public interface IPresenceService
{
    Task SetUserStatusAsync(Guid userId, UserStatus status);
    Task<UserStatus> GetUserStatusAsync(Guid userId);
    Task<DateTime?> GetLastSeenAsync(Guid userId);
    Task<IEnumerable<Guid>> GetOnlineUsersAsync(IEnumerable<Guid> userIds);
}

public class PresenceService : IPresenceService
{
    private readonly IRedisService _redis;
    private const string StatusPrefix = "user:{0}:status";
    private const string LastSeenPrefix = "user:{0}:lastSeen";

    public PresenceService(IRedisService redis)
    {
        _redis = redis;
    }

    public async Task SetUserStatusAsync(Guid userId, UserStatus status)
    {
        var statusKey = string.Format(StatusPrefix, userId);
        var lastSeenKey = string.Format(LastSeenPrefix, userId);
        
        await _redis.SetAsync(statusKey, status.ToString());
        await _redis.SetAsync(lastSeenKey, DateTime.UtcNow.ToString("O")); // ISO 8601
    }

    public async Task<UserStatus> GetUserStatusAsync(Guid userId)
    {
        var key = string.Format(StatusPrefix, userId);
        var value = await _redis.GetAsync(key);
        
        if (Enum.TryParse<UserStatus>(value, out var status))
        {
            return status;
        }
        
        return UserStatus.Offline;
    }

    public async Task<DateTime?> GetLastSeenAsync(Guid userId)
    {
        var key = string.Format(LastSeenPrefix, userId);
        var value = await _redis.GetAsync(key);
        
        if (DateTime.TryParse(value, out var lastSeen))
        {
            return lastSeen;
        }
        
        return null;
    }

    public async Task<IEnumerable<Guid>> GetOnlineUsersAsync(IEnumerable<Guid> userIds)
    {
        var onlineUsers = new List<Guid>();
        foreach (var userId in userIds)
        {
            if (await GetUserStatusAsync(userId) == UserStatus.Online)
            {
                onlineUsers.Add(userId);
            }
        }
        return onlineUsers;
    }
}
