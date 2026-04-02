using System.Security.Claims;
using ChatSystem.Application.DTOs.Chat;
using ChatSystem.Application.DTOs.Events;
using ChatSystem.Application.Interfaces;
using ChatSystem.Application.Interfaces.Services;
using ChatSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.API.Hubs;

/// <summary>
/// The WebSocket Gateway for the Chat System.
/// Responsibilities: WebSocket management, Auth validation, Event publishing.
/// In this event-driven architecture, the Hub publishes raw events to Kafka.
/// It does NOT write directly to the database.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IConnectionTracker _tracker;
    private readonly IPresenceService _presenceService;
    private readonly IEventProducer _eventProducer;

    public ChatHub(
        IChatService chatService, 
        IConnectionTracker tracker, 
        IPresenceService presenceService,
        IEventProducer eventProducer)
    {
        _chatService = chatService;
        _tracker = tracker;
        _presenceService = presenceService;
        _eventProducer = eventProducer;
    }

    /// <summary>
    /// Handles a new client connection, updates local/global presence, and syncs initial state.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await _tracker.AddConnection(userId, Context.ConnectionId);
        await _presenceService.SetUserStatusAsync(userId, UserStatus.Online);
        
        // Publish presence event to Kafka
        await _eventProducer.PublishAsync("user.presence", userId.ToString(), new SystemEvent(
            "user.online", userId, GetUsername(), null, DateTime.UtcNow));

        // Local state sync for the caller
        var onlineUsers = await _tracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("InitialOnlineUsers", onlineUsers);
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Handles client disconnection and updates global offline status if no active connections remain.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await _tracker.RemoveConnection(userId, Context.ConnectionId);

        if (!await _tracker.IsUserOnline(userId))
        {
            await _presenceService.SetUserStatusAsync(userId, UserStatus.Offline);
            
            // Publish presence event to Kafka
            await _eventProducer.PublishAsync("user.presence", userId.ToString(), new SystemEvent(
                "user.offline", userId, GetUsername(), null, DateTime.UtcNow));
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Adds the current connection to a specific chat group for targeted broadcasting.
    /// </summary>
    public async Task JoinChat(string chatId)
    {
        var userId = GetUserId();
        // Still use service for basic membership validation at the gateway level
        if (await _chatService.GetChatAsync(userId, Guid.Parse(chatId)) != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }
    }

    /// <summary>
    /// Removes the current connection from a chat group.
    /// </summary>
    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
    }

    /// <summary>
    /// Publishes a message to Kafka for asynchronous processing and persistence.
    /// Does NOT write to the DB directly.
    /// </summary>
    public async Task SendMessage(string chatId, string message)
    {
        var userId = GetUserId();
        var chatGuid = Guid.Parse(chatId);

        Console.WriteLine($"[Gateway] Received message from {userId} for chat {chatId}: {message}");

        // Publish to Kafka. We partition by chatId to ensure message order.
        await _eventProducer.PublishAsync("message.send", chatId, new MessageSendEvent(
            chatGuid, userId, message, DateTime.UtcNow));
    }

    /// <summary>
    /// Broadcasts a typing notification to other members of the chat group via Kafka.
    /// </summary>
    public async Task SendTyping(string chatId)
    {
        var userId = GetUserId();
        var username = GetUsername();
        var chatGuid = Guid.Parse(chatId);

        await _eventProducer.PublishAsync("typing.events", chatId, new SystemEvent(
            "typing.started", userId, username, chatGuid, DateTime.UtcNow));
    }

    private Guid GetUserId()
    {
        var idClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim == null) throw new HubException("Unauthorized");
        return Guid.Parse(idClaim.Value);
    }

    private string? GetUsername() => Context.User?.FindFirst(ClaimTypes.GivenName)?.Value;
}
