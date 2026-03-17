using ChatSystem.Domain.Entities;
using ChatSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Infrastructure.Persistence;

public class ChatDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Chat> Chats { get; set; } = null!;
    public DbSet<ChatMember> ChatMembers { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // ChatMember Composite Key
        modelBuilder.Entity<ChatMember>()
            .HasKey(cm => new { cm.ChatId, cm.UserId });

        modelBuilder.Entity<ChatMember>()
            .HasOne(cm => cm.Chat)
            .WithMany(c => c.Members)
            .HasForeignKey(cm => cm.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMember>()
            .HasOne(cm => cm.User)
            .WithMany(u => u.ChatMembers)
            .HasForeignKey(cm => cm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Message
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting user from deleting messages immediately? Or Cascade? Usually keep history.
            // Let's use Restrict for now, or SetNull if SenderId is nullable. Since it's required, restrict.
            // Actually, if a user is deleted, their messages should probably remain as "Deleted User" or similar.
            // But for simplicity, let's stick to Cascade or Restrict. Restrict is safer.
    }
}
