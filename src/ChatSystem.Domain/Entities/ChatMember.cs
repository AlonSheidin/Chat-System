using System;
using ChatSystem.Domain.Enums;

namespace ChatSystem.Domain.Entities;

public class ChatMember
{
    public Guid ChatId { get; set; }
    public Guid UserId { get; set; }
    public MemberRole Role { get; set; } = MemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Chat Chat { get; set; } = null!;
    public User User { get; set; } = null!;
}
