using ChatSystem.Application.Interfaces.Repositories;
using ChatSystem.Domain.Entities;
using ChatSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly ChatDbContext _context;

    public ChatRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<Chat?> GetByIdAsync(Guid id)
    {
        return await _context.Chats
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Chat chat)
    {
        await _context.Chats.AddAsync(chat);
    }

    public async Task AddMemberAsync(ChatMember member)
    {
        await _context.ChatMembers.AddAsync(member);
    }

    public async Task<bool> IsMemberAsync(Guid chatId, Guid userId)
    {
        return await _context.ChatMembers
            .AnyAsync(cm => cm.ChatId == chatId && cm.UserId == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
