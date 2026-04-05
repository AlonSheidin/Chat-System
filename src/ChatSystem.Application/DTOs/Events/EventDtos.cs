namespace ChatSystem.Application.DTOs.Events;

/// <summary>
/// Event emitted when a user sends a message through the gateway.
/// </summary>
public record MessageSendEvent(
    Guid MessageId,
    Guid ChatId,
    Guid SenderId,
    string Content,
    DateTime OccurredAt
);

/// <summary>
/// Event emitted after a message has been successfully persisted to the database.
/// </summary>
public record MessageStoredEvent(
    Guid Id,
    Guid ChatId,
    Guid SenderId,
    string SenderName,
    string Content,
    DateTime SentAt
);

/// <summary>
/// Event emitted for presence and typing indicators.
/// </summary>
public record SystemEvent(
    string Type, // user.online, user.offline, typing.started, typing.stopped
    Guid UserId,
    string? Username,
    Guid? ChatId,
    DateTime OccurredAt
);
