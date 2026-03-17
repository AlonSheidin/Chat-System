using System;

namespace ChatSystem.Domain.Entities;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public required string Content { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Chat Chat { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
