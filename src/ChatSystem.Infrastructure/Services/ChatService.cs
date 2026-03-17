using ChatSystem.Application.DTOs.Chat;
using ChatSystem.Application.Interfaces.Repositories;
using ChatSystem.Application.Interfaces.Services;
using ChatSystem.Domain.Entities;
using ChatSystem.Domain.Enums;

namespace ChatSystem.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;

    public ChatService(IChatRepository chatRepository, IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
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

    public async Task<MessageResponse> SendMessageAsync(Guid userId, Guid chatId, SendMessageRequest request)
    {
        if (!await _chatRepository.IsMemberAsync(chatId, userId))
        {
            throw new UnauthorizedAccessException("User is not a member of this chat.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new Exception("User not found");

        var message = new Message
        {
            ChatId = chatId,
            SenderId = userId,
            Content = request.Content
        };

        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();

        return new MessageResponse(message.Id, message.ChatId, message.SenderId, user.Username, message.Content, message.SentAt);
    }

    public async Task<IEnumerable<MessageResponse>> GetMessagesAsync(Guid userId, Guid chatId)
    {
        if (!await _chatRepository.IsMemberAsync(chatId, userId))
        {
            throw new UnauthorizedAccessException("User is not a member of this chat.");
        }

        var messages = await _messageRepository.GetByChatIdAsync(chatId);

        return messages.Select(m => new MessageResponse(
            m.Id, m.ChatId, m.SenderId, m.Sender.Username, m.Content, m.SentAt
        ));
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
