using ChatSystem.Application.DTOs.Chat;

namespace ChatSystem.Application.Interfaces.Services;

/// <summary>
/// Service for managing chats, members, and messages.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Creates a new private or group chat.
    /// </summary>
    Task<ChatResponse> CreateChatAsync(Guid userId, CreateChatRequest request);

    /// <summary>
    /// Retrieves all chats the specified user is a member of.
    /// </summary>
    Task<IEnumerable<ChatResponse>> GetUserChatsAsync(Guid userId);

    /// <summary>
    /// Adds a new user to an existing group chat.
    /// </summary>
    Task AddMemberToChatAsync(Guid userId, Guid chatId, Guid newMemberId);

    /// <summary>
    /// Persists a new message to the database and updates the cache.
    /// </summary>
    Task<MessageResponse> SendMessageAsync(Guid userId, Guid chatId, SendMessageRequest request);

    /// <summary>
    /// Retrieves recent messages for a chat, using cache-aside pattern.
    /// </summary>
    Task<IEnumerable<MessageResponse>> GetMessagesAsync(Guid userId, Guid chatId);

    /// <summary>
    /// Gets details for a specific chat if the user is a member.
    /// </summary>
    Task<ChatResponse?> GetChatAsync(Guid userId, Guid chatId);
}
