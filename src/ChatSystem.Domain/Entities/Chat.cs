using System;
using System.Collections.Generic;

namespace ChatSystem.Domain.Entities;

public class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Name { get; set; }
    public bool IsGroup { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
