using System.Security.Claims;
using ChatSystem.Application.DTOs.Chat;
using ChatSystem.Application.DTOs.Events;
using ChatSystem.Application.Interfaces.Services;
using ChatSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.API.Hubs;

/// <summary>
/// SignalR Hub for handling real-time WebSocket communication.
/// In this distributed architecture, the Hub publishes events to Redis Pub/Sub 
/// which are then picked up by all instances via the SignalRDispatcher.
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IConnectionTracker _tracker;
    private readonly IPresenceService _presenceService;
    private readonly IPubSubService _pubSub;

    public ChatHub(
        IChatService chatService, 
        IConnectionTracker tracker, 
        IPresenceService presenceService,
        IPubSubService pubSub)
    {
        _chatService = chatService;
        _tracker = tracker;
        _presenceService = presenceService;
        _pubSub = pubSub;
    }

    /// <summary>
    /// Handles a new client connection, updates global presence, and syncs initial state.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await _tracker.AddConnection(userId, Context.ConnectionId);
        await _presenceService.SetUserStatusAsync(userId, UserStatus.Online);
        
        // Notify others globally
        await _pubSub.PublishAsync("user:presence", new UserPresenceEvent { UserId = userId, Status = "Online" });

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
            await _pubSub.PublishAsync("user:presence", new UserPresenceEvent { UserId = userId, Status = "Offline" });
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Adds the current connection to a specific chat group for targeted broadcasting.
    /// </summary>
    public async Task JoinChat(string chatId)
    {
        var userId = GetUserId();
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
    /// Saves a message to the database and publishes it to the global Redis channel for delivery.
    /// </summary>
    public async Task SendMessage(string chatId, string message)
    {
        var userId = GetUserId();
        var response = await _chatService.SendMessageAsync(userId, Guid.Parse(chatId), new SendMessageRequest(message));
        
        // Publish to global Redis channel
        await _pubSub.PublishAsync("chat:messages", new ChatMessageEvent { Message = response });
    }

    /// <summary>
    /// Broadcasts a typing notification to other members of the chat group.
    /// </summary>
    public async Task SendTyping(string chatId)
    {
        var userId = GetUserId();
        var username = Context.User?.FindFirst(ClaimTypes.GivenName)?.Value;

        // Publish to global Redis channel
        await _pubSub.PublishAsync("chat:typing", new UserTypingEvent 
        { 
            ChatId = chatId, 
            UserId = userId, 
            Username = username ?? "Unknown" 
        });
    }

    private Guid GetUserId()
    {
        var idClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim == null) throw new HubException("Unauthorized");
        return Guid.Parse(idClaim.Value);
    }
}
