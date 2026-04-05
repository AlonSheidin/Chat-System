using ChatSystem.Application.DTOs.Chat;
using ChatSystem.Application.Interfaces.Repositories;
using ChatSystem.Application.Interfaces.Services;
using ChatSystem.Domain.Entities;
using ChatSystem.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ChatSystem.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageCache _messageCache;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IChatRepository chatRepository, 
        IMessageRepository messageRepository, 
        IUserRepository userRepository,
        IMessageCache messageCache,
        ILogger<ChatService> logger)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _messageCache = messageCache;
        _logger = logger;
    }

    public async Task<ChatResponse> CreateChatAsync(Guid userId, CreateChatRequest request)
    {
        var chat = new Chat
        {
            Name = request.Name,
            IsGroup = request.IsGroup
        };

        await _chatRepository.AddAsync(chat);

        // Add creator
        await _chatRepository.AddMemberAsync(new ChatMember
        {
            ChatId = chat.Id,
            UserId = userId,
            Role = MemberRole.Admin
        });

        // Add other members
        if (request.MemberIds != null)
        {
            foreach (var memberId in request.MemberIds)
            {
                if (memberId != userId) // Avoid duplicate
                {
                    await _chatRepository.AddMemberAsync(new ChatMember
                    {
                        ChatId = chat.Id,
                        UserId = memberId,
                        Role = MemberRole.Member
                    });
                }
            }
        }

        await _chatRepository.SaveChangesAsync();

        return new ChatResponse(chat.Id, chat.Name, chat.IsGroup, 
            request.MemberIds?.Concat(new[] { userId }).Distinct().ToList() ?? new List<Guid> { userId });
    }

    public async Task<IEnumerable<ChatResponse>> GetUserChatsAsync(Guid userId)
    {
        var chats = await _chatRepository.GetByUserIdAsync(userId);
        return chats.Select(c => new ChatResponse(
            c.Id,
            c.Name,
            c.IsGroup,
            c.Members.Select(m => m.UserId).ToList()
        ));
    }

    public async Task AddMemberToChatAsync(Guid userId, Guid chatId, Guid newMemberId)
    {
        // 1. Check if the current user is a member/admin of the chat
        if (!await _chatRepository.IsMemberAsync(chatId, userId))
        {
            throw new UnauthorizedAccessException("User is not a member of this chat.");
        }

        // 2. Check if the chat exists and is a group
        var chat = await _chatRepository.GetByIdAsync(chatId);
        if (chat == null) throw new Exception("Chat not found.");
        if (!chat.IsGroup) throw new Exception("Cannot add members to a private chat.");

        // 3. Check if the new member is already there
        if (await _chatRepository.IsMemberAsync(chatId, newMemberId))
        {
            throw new Exception("User is already a member of this chat.");
        }

        // 4. Add the member
        await _chatRepository.AddMemberAsync(new ChatMember
        {
            ChatId = chatId,
            UserId = newMemberId,
            Role = MemberRole.Member
        });

        await _chatRepository.SaveChangesAsync();
    }

    public async Task<MessageResponse> SendMessageAsync(Guid userId, Guid chatId, SendMessageRequest request, Guid? messageId = null)
    {
        if (!await _chatRepository.IsMemberAsync(chatId, userId))
        {
            throw new UnauthorizedAccessException("User is not a member of this chat.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var message = new Message
        {
            Id = messageId ?? Guid.NewGuid(),
            ChatId = chatId,
            SenderId = userId,
            Content = request.Content
        };

        try
        {
            await _messageRepository.AddAsync(message);
            await _messageRepository.SaveChangesAsync();
        }
        catch (Exception ex) when (ex.InnerException?.Message.Contains("duplicate key") == true || ex.Message.Contains("duplicate key"))
        {
            // Idempotency: If the message already exists (e.g. Kafka retry), just return the existing one.
            _logger.LogWarning("Duplicate message detected for Id {MessageId}. Skipping persistence.", message.Id);
            // In a real app, we'd fetch the existing record to return correct SentAt, but for now we proceed.
        }

        var response = new MessageResponse(message.Id, message.ChatId, message.SenderId, user.Username, message.Content, message.SentAt);

        // Update Cache (Fail-safe)
        try
        {
            await _messageCache.AddMessageAsync(chatId, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update message cache for chat {ChatId}", chatId);
        }

        return response;
    }

    public async Task<IEnumerable<MessageResponse>> GetMessagesAsync(Guid userId, Guid chatId)
    {
        if (!await _chatRepository.IsMemberAsync(chatId, userId))
        {
            throw new UnauthorizedAccessException("User is not a member of this chat.");
        }

        // 1. Check Cache
        try
        {
            var cachedMessages = await _messageCache.GetRecentMessagesAsync(chatId);
            if (cachedMessages != null)
            {
                return cachedMessages;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get messages from cache for chat {ChatId}", chatId);
        }

        // 2. Fetch from DB
        var messages = await _messageRepository.GetByChatIdAsync(chatId);
        var response = messages.Select(m => new MessageResponse(
            m.Id, m.ChatId, m.SenderId, m.Sender.Username, m.Content, m.SentAt
        )).ToList();

        // 3. Store in Cache
        try
        {
            await _messageCache.SetRecentMessagesAsync(chatId, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set message cache for chat {ChatId}", chatId);
        }

        return response;
    }
    
    public async Task<ChatResponse?> GetChatAsync(Guid userId, Guid chatId)
    {
        if (!await _chatRepository.IsMemberAsync(chatId, userId))
        {
            return null; // or throw
        }

        var chat = await _chatRepository.GetByIdAsync(chatId);
        if (chat == null) return null;

        return new ChatResponse(chat.Id, chat.Name, chat.IsGroup, chat.Members.Select(m => m.UserId).ToList());
    }
}
