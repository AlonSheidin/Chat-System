using System.Text.Json;
using ChatSystem.Application.DTOs.Chat;

namespace ChatSystem.Infrastructure.Services;

public interface IMessageCache
{
    Task<IEnumerable<MessageResponse>?> GetRecentMessagesAsync(Guid chatId);
    Task SetRecentMessagesAsync(Guid chatId, IEnumerable<MessageResponse> messages);
    Task AddMessageAsync(Guid chatId, MessageResponse message);
    Task InvalidateAsync(Guid chatId);
}

public class MessageCache : IMessageCache
{
    private readonly IRedisService _redis;
    private const string CacheKeyPrefix = "chat:{0}:recent_messages";
    private const int MaxMessages = 50;
    private readonly TimeSpan _ttl = TimeSpan.FromMinutes(10);

    public MessageCache(IRedisService redis)
    {
        _redis = redis;
    }

    public async Task<IEnumerable<MessageResponse>?> GetRecentMessagesAsync(Guid chatId)
    {
        var key = string.Format(CacheKeyPrefix, chatId);
        var cachedValues = await _redis.ListGetAsync(key);
        
        if (cachedValues == null || !cachedValues.Any())
        {
            return null;
        }

        return cachedValues.Select(v => JsonSerializer.Deserialize<MessageResponse>(v)!)
                           .OrderByDescending(m => m.SentAt);
    }

    public async Task SetRecentMessagesAsync(Guid chatId, IEnumerable<MessageResponse> messages)
    {
        var key = string.Format(CacheKeyPrefix, chatId);
        await InvalidateAsync(chatId);

        // Reverse because we want newest at the end of the list (RightPush)
        // and GetRecentMessages orders by Descending for the UI
        foreach (var message in messages.OrderBy(m => m.SentAt).TakeLast(MaxMessages))
        {
            await AddMessageAsync(chatId, message);
        }
        
        await _redis.GetDatabase().KeyExpireAsync(key, _ttl);
    }

    public async Task AddMessageAsync(Guid chatId, MessageResponse message)
    {
        var key = string.Format(CacheKeyPrefix, chatId);
        var value = JsonSerializer.Serialize(message);
        
        await _redis.ListAppendAsync(key, value);
        await _redis.ListTrimAsync(key, -MaxMessages, -1); // Keep only last 50
        await _redis.GetDatabase().KeyExpireAsync(key, _ttl);
    }

    public async Task InvalidateAsync(Guid chatId)
    {
        var key = string.Format(CacheKeyPrefix, chatId);
        await _redis.RemoveAsync(key);
    }
}
