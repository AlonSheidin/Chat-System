using ChatSystem.Domain.Entities;

namespace ChatSystem.Application.Interfaces.Repositories;

public interface IChatRepository
{
    Task<Chat?> GetByIdAsync(Guid id);
    Task AddAsync(Chat chat);
    Task AddMemberAsync(ChatMember member);
    Task<bool> IsMemberAsync(Guid chatId, Guid userId);
    Task SaveChangesAsync();
}
