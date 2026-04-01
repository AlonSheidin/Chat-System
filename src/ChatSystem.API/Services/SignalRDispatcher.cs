using ChatSystem.API.Hubs;
using ChatSystem.Application.DTOs.Events;
using ChatSystem.Infrastructure.Services;
using Microsoft.AspNetCore.SignalR;

namespace ChatSystem.API.Services;

/// <summary>
/// Background worker that enables horizontal scaling by listening for distributed events 
/// on Redis Pub/Sub and broadcasting them to locally connected SignalR clients.
/// </summary>
public class SignalRDispatcher : BackgroundService
{
    private readonly IPubSubService _pubSub;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<SignalRDispatcher> _logger;
    private readonly string _instanceId = Guid.NewGuid().ToString();

    public SignalRDispatcher(
        IPubSubService pubSub, 
        IHubContext<ChatHub> hubContext,
        ILogger<SignalRDispatcher> logger)
    {
        _pubSub = pubSub;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SignalR Dispatcher started with Instance ID: {InstanceId}", _instanceId);

        // 1. Listen for Chat Messages
        await _pubSub.SubscribeAsync<ChatMessageEvent>("chat:messages", async (evt) =>
        {
            _logger.LogDebug("Received distributed message for chat {ChatId}", evt.Message.ChatId);
            await _hubContext.Clients.Group(evt.Message.ChatId.ToString()).SendAsync("ReceiveMessage", evt.Message);
        });

        // 2. Listen for Presence
        await _pubSub.SubscribeAsync<UserPresenceEvent>("user:presence", async (evt) =>
        {
            var methodName = evt.Status == "Online" ? "UserOnline" : "UserOffline";
            // Note: In IHubContext, we broadcast to All since we don't have a 'caller' concept
            await _hubContext.Clients.All.SendAsync(methodName, evt.UserId);
        });

        // 3. Listen for Typing
        await _pubSub.SubscribeAsync<UserTypingEvent>("chat:typing", async (evt) =>
        {
            await _hubContext.Clients.Group(evt.ChatId).SendAsync("UserTyping", new 
            {
                ChatId = evt.ChatId,
                UserId = evt.UserId,
                Username = evt.Username
            });
        });

        // Keep the service alive
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
