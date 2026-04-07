using ChatSystem.Application.Common;
using System.Text.Json;
using ChatSystem.Application.DTOs.Chat;
using ChatSystem.Application.DTOs.Events;
using ChatSystem.Application.Interfaces;
using ChatSystem.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChatSystem.Infrastructure.Workers;

/// <summary>
/// Background worker responsible for persisting messages to the database.
/// Consumes: message.send
/// Produces: message.stored
/// </summary>
public class MessageWorker : BackgroundService
{
    private readonly IEventConsumer _consumer;
    private readonly IEventProducer _producer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessageWorker> _logger;

    public MessageWorker(
        IEventConsumer consumer, 
        IEventProducer producer, 
        IServiceProvider serviceProvider,
        ILogger<MessageWorker> logger)
    {
        _consumer = consumer;
        _producer = producer;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Executes the consumption loop for message persistence.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MessageWorker starting...");

        await _consumer.ConsumeAsync(new[] { KafkaTopics.MessageSend }, async (key, value) =>
        {
            try
            {
                var sendEvent = JsonSerializer.Deserialize<MessageSendEvent>(value);
                if (sendEvent == null) return;

                using var scope = _serviceProvider.CreateScope();
                var chatService = scope.ServiceProvider.GetRequiredService<IChatService>();

                // 1. Persist to PostgreSQL using the pre-generated Id for idempotency
                var response = await chatService.SendMessageAsync(
                    sendEvent.SenderId, 
                    sendEvent.ChatId, 
                    new SendMessageRequest(sendEvent.Content),
                    sendEvent.MessageId);

                // 2. Emit message.stored event
                var storedEvent = new MessageStoredEvent(
                    response.Id,
                    response.ChatId,
                    response.SenderId,
                    response.SenderName,
                    response.Content,
                    response.SentAt);

                await _producer.PublishAsync(KafkaTopics.MessageStored, key, storedEvent);
                
                _logger.LogInformation("Message {MessageId} saved to DB and 'message.stored' event emitted.", sendEvent.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message.send event for key: {Key}", key);
            }
        }, stoppingToken);
    }
}
