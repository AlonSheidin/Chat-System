using ChatSystem.Domain.Entities;

namespace ChatSystem.Application.Interfaces.Repositories;

public interface IChatRepository
{
    Task<Chat?> GetByIdAsync(Guid id);
    Task<IEnumerable<Chat>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Chat chat);
    Task AddMemberAsync(ChatMember member);
    Task<bool> IsMemberAsync(Guid chatId, Guid userId);
    Task SaveChangesAsync();
}
