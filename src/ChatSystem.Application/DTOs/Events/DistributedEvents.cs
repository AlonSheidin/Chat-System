using ChatSystem.Application.DTOs.Chat;

namespace ChatSystem.Application.DTOs.Events;

public abstract class BaseDistributedEvent
{
    public string OriginInstanceId { get; set; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

public class ChatMessageEvent : BaseDistributedEvent
{
    public MessageResponse Message { get; set; } = null!;
}

public class UserPresenceEvent : BaseDistributedEvent
{
    public Guid UserId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class UserTypingEvent : BaseDistributedEvent
{
    public string ChatId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
}
