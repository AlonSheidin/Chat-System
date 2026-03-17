using ChatSystem.Application.Interfaces.Repositories;
using ChatSystem.Domain.Entities;
using ChatSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ChatDbContext _context;

    public MessageRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
    }

    public async Task<IEnumerable<Message>> GetByChatIdAsync(Guid chatId, int skip = 0, int take = 50)
    {
        return await _context.Messages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.SentAt) // Get latest first
            .Skip(skip)
            .Take(take)
            .Include(m => m.Sender)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
