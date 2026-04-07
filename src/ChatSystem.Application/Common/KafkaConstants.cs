namespace ChatSystem.Application.Common;

/// <summary>
/// Centralized, type-safe definitions for all Kafka topics used in the system.
/// Prevents "magic string" bugs during event production and consumption.
/// </summary>
public static class KafkaTopics
{
    public const string MessageSend = "message.send";
    public const string MessageStored = "message.stored";
    public const string UserOnline = "user.online";
    public const string UserOffline = "user.offline";
    public const string TypingStarted = "typing.started";
    public const string TypingStopped = "typing.stopped";

    /// <summary>
    /// Returns all topics required for infrastructure initialization.
    /// </summary>
    public static readonly string[] All = 
    { 
        MessageSend, 
        MessageStored, 
        UserOnline, 
        UserOffline, 
        TypingStarted, 
        TypingStopped 
    };

    /// <summary>
    /// Returns topics the Notification Worker (Broadcast) should subscribe to.
    /// </summary>
    public static readonly string[] Notifications = 
    { 
        MessageStored, 
        UserOnline, 
        UserOffline, 
        TypingStarted, 
        TypingStopped 
    };
}

/// <summary>
/// Standardized types for events that travel through SystemEvent payloads.
/// </summary>
public static class SystemEventTypes
{
    public const string UserOnline = "user.online";
    public const string UserOffline = "user.offline";
    public const string TypingStarted = "typing.started";
    public const string TypingStopped = "typing.stopped";
}
