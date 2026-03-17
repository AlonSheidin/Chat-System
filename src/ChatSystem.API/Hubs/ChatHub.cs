using System.Security.Claims;
using ChatSystem.Application.DTOs.Chat;
using ChatSystem.Application.Interfaces.Services;
using ChatSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IConnectionTracker _tracker;

    public ChatHub(IChatService chatService, IConnectionTracker tracker)
    {
        _chatService = chatService;
        _tracker = tracker;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        await _tracker.AddConnection(userId, Context.ConnectionId);
        
        // 1. Notify others that user is online
        await Clients.Others.SendAsync("UserOnline", userId);

        // 2. Send the current list of online users to the caller
        var onlineUsers = await _tracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("InitialOnlineUsers", onlineUsers);
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        await _tracker.RemoveConnection(userId, Context.ConnectionId);

        if (!await _tracker.IsUserOnline(userId))
        {
            await Clients.Others.SendAsync("UserOffline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(string chatId)
    {
        var userId = GetUserId();
        if (await _chatService.GetChatAsync(userId, Guid.Parse(chatId)) != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }
    }

    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task SendMessage(string chatId, string message)
    {
        var userId = GetUserId();
        var response = await _chatService.SendMessageAsync(userId, Guid.Parse(chatId), new SendMessageRequest(message));
        await Clients.Group(chatId).SendAsync("ReceiveMessage", response);
    }

    public async Task SendTyping(string chatId)
    {
        var userId = GetUserId();
        var username = Context.User?.FindFirst(ClaimTypes.GivenName)?.Value;

        await Clients.Group(chatId).SendAsync("UserTyping", new 
        {
            ChatId = chatId,
            UserId = userId,
            Username = username
        });
    }

    private Guid GetUserId()
    {
        var idClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim == null) throw new HubException("Unauthorized");
        return Guid.Parse(idClaim.Value);
    }
}
