using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.DTOs.Chat;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatSystem.IntegrationTests;

public class ChatHubTests : TestBase
{
    public ChatHubTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private async Task<AuthResponse> RegisterAndGetAuth(HttpClient client, string username, string email)
    {
        var request = new RegisterRequest(username, email, "Password123!");
        var response = await client.PostAsJsonAsync("/auth/register", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    [Fact]
    public async Task SendMessage_ShouldBroadcastToGroupAndSaveToDb()
    {
        // Arrange
        var client = Factory.CreateClient();
        var auth = await RegisterAndGetAuth(client, "hubuser", "hub@example.com");
        
        // Create a chat first
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        var chatResponse = await client.PostAsJsonAsync("/chats", new CreateChatRequest("Hub Chat", true, new List<Guid>()));
        var chat = await chatResponse.Content.ReadFromJsonAsync<ChatResponse>();

        // Build SignalR connection
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/ws", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(auth.Token)!;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        MessageResponse? receivedMsg = null;
        connection.On<MessageResponse>("ReceiveMessage", msg => receivedMsg = msg);

        await connection.StartAsync();
        await connection.InvokeAsync("JoinChat", chat!.Id.ToString());

        // Act
        await connection.InvokeAsync("SendMessage", chat.Id.ToString(), "Hello from Hub!");

        // Assert
        // Wait a bit for the async broadcast via Kafka mock loopback
        for (int i = 0; i < 20 && receivedMsg == null; i++) await Task.Delay(100);

        receivedMsg.Should().NotBeNull();
        receivedMsg!.Content.Should().Be("Hello from Hub!");
        receivedMsg.SenderId.Should().Be(auth.Id);

        // Verify it was saved in DB via REST API
        var msgResponse = await client.GetAsync($"/chats/{chat.Id}/messages");
        var messages = await msgResponse.Content.ReadFromJsonAsync<IEnumerable<MessageResponse>>();
        messages.Should().Contain(m => m.Content == "Hello from Hub!");

        await connection.StopAsync();
    }
}
