using System;
using System.Collections.Generic;

namespace ChatSystem.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ChatMember> ChatMembers { get; set; } = new List<ChatMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
