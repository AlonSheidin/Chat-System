using System.ComponentModel.DataAnnotations;

namespace ChatSystem.Application.DTOs.Chat;

public class CreateChatRequest
{
    [Required] 
    public string Name { get; set; } = string.Empty;
    public bool IsGroup { get; set; }
    public List<Guid> MemberIds { get; set; } = new();
}

public class SendMessageRequest
{
    [Required] 
    public string Content { get; set; } = string.Empty;

    public SendMessageRequest() { }
    public SendMessageRequest(string content) => Content = content;
}

public class AddMemberRequest
{
    [Required] 
    public Guid UserId { get; set; }
}

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
