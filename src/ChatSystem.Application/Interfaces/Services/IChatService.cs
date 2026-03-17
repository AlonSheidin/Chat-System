using ChatSystem.Application.DTOs.Chat;

namespace ChatSystem.Application.Interfaces.Services;

public interface IChatService
{
    Task<ChatResponse> CreateChatAsync(Guid userId, CreateChatRequest request);
    Task<MessageResponse> SendMessageAsync(Guid userId, Guid chatId, SendMessageRequest request);
    Task<IEnumerable<MessageResponse>> GetMessagesAsync(Guid userId, Guid chatId);
    Task<ChatResponse?> GetChatAsync(Guid userId, Guid chatId);
}
