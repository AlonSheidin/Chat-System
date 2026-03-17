using System.ComponentModel.DataAnnotations;

namespace ChatSystem.Application.DTOs.Chat;

public record CreateChatRequest(
    [Required] string Name, // Optional for private, but let's require for groups
    bool IsGroup,
    List<Guid> MemberIds
);

public record SendMessageRequest(
    [Required] string Content
);

public record ChatResponse(
    Guid Id,
    string? Name,
    bool IsGroup,
    List<Guid> MemberIds
);

public record MessageResponse(
    Guid Id,
    Guid ChatId,
    Guid SenderId,
    string SenderName,
    string Content,
    DateTime SentAt
);
