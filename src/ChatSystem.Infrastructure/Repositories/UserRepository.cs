using ChatSystem.Application.Interfaces.Repositories;
using ChatSystem.Domain.Entities;
using ChatSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ChatDbContext _context;

    public UserRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task<IEnumerable<User>> SearchAsync(string query)
    {
        return await _context.Users
            .Where(u => u.Username.Contains(query) || u.Email.Contains(query))
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
