using System.Text.Json;
using ChatSystem.Application.DTOs.Events;
using ChatSystem.Application.Interfaces;
using ChatSystem.API.Hubs;
using ChatSystem.Infrastructure.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatSystem.API.Workers;

/// <summary>
/// Background worker responsible for broadcasting events to connected WebSocket clients.
/// Consumes: message.stored, user.presence, typing.events
/// Note: To ensure all instances receive broadcast events, each instance uses a unique GroupId.
/// </summary>
public class NotificationWorker : BackgroundService
{
    private readonly IEventConsumer _consumer;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<NotificationWorker> _logger;
    private readonly KafkaOptions _kafkaOptions;

    public NotificationWorker(
        IEventConsumer consumer, 
        IHubContext<ChatHub> hubContext, 
        ILogger<NotificationWorker> logger,
        IOptions<KafkaOptions> kafkaOptions)
    {
        _consumer = consumer;
        _hubContext = hubContext;
        _logger = logger;
        _kafkaOptions = kafkaOptions.Value;
    }

    /// <summary>
    /// Executes the consumption loop for real-time notifications.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationWorker starting...");

        // For broadcasting, we need each instance to be in its own group
        var uniqueGroupId = $"{_kafkaOptions.GroupId}-broadcast-{Guid.NewGuid()}";
        var topics = new[] { "message.stored", "user.online", "user.offline", "typing.started", "typing.stopped" };

        await _consumer.ConsumeAsync(topics, async (key, value) =>
        {
            try
            {
                if (value.Contains("\"Content\"")) // MessageStoredEvent
                {
                    await HandleMessageStored(value);
                }
                else if (value.Contains("\"user.online\"") || value.Contains("\"user.offline\""))
                {
                    await HandlePresence(value);
                }
                else if (value.Contains("\"typing.started\"") || value.Contains("\"typing.stopped\""))
                {
                    await HandleTyping(value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification event");
            }
        }, stoppingToken, uniqueGroupId);
    }

    private async Task HandleMessageStored(string json)
    {
        var @event = JsonSerializer.Deserialize<MessageStoredEvent>(json);
        if (@event == null) return;

        _logger.LogInformation("Broadcasting message from {SenderName} to group {ChatId}", @event.SenderName, @event.ChatId);
        // Push to everyone in the chat group connected to THIS instance
        await _hubContext.Clients.Group(@event.ChatId.ToString()).SendAsync("ReceiveMessage", @event);
    }

    private async Task HandlePresence(string json)
    {
        var @event = JsonSerializer.Deserialize<SystemEvent>(json);
        if (@event == null) return;

        var methodName = @event.Type == "user.online" ? "UserOnline" : "UserOffline";
        await _hubContext.Clients.All.SendAsync(methodName, @event.UserId);
    }

    private async Task HandleTyping(string json)
    {
        var @event = JsonSerializer.Deserialize<SystemEvent>(json);
        if (@event == null || @event.ChatId == null) return;

        await _hubContext.Clients.Group(@event.ChatId.Value.ToString()).SendAsync("UserTyping", new 
        {
            ChatId = @event.ChatId,
            UserId = @event.UserId,
            Username = @event.Username
        });
    }
}
