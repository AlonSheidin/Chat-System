using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // Optionally map user to connection
        await base.OnConnectedAsync();
    }

    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task SendMessage(string chatId, string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = Context.User?.FindFirst(ClaimTypes.GivenName)?.Value;

        // In a real app, save to DB here via Service
        // For now, just broadcast
        await Clients.Group(chatId).SendAsync("ReceiveMessage", new 
        {
            SenderId = userId,
            SenderName = username,
            Content = message,
            SentAt = DateTime.UtcNow
        });
    }
}
