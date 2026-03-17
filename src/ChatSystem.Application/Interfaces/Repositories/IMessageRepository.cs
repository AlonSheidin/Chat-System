using ChatSystem.Domain.Entities;

namespace ChatSystem.Application.Interfaces.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<IEnumerable<Message>> GetByChatIdAsync(Guid chatId, int skip = 0, int take = 50);
    Task SaveChangesAsync();
}
