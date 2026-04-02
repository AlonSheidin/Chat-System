using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.DTOs.Chat;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatSystem.IntegrationTests;

public class PresenceTests : TestBase
{
    public PresenceTests(WebApplicationFactory<Program> factory) : base(factory)
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
    public async Task ConnectingToHub_ShouldBroadcastOnlineStatus()
    {
        // Arrange
        var client1 = Factory.CreateClient();
        var auth1 = await RegisterAndGetAuth(client1, "user1", "u1@test.com");

        var client2 = Factory.CreateClient();
        var auth2 = await RegisterAndGetAuth(client2, "user2", "u2@test.com");

        var connection2 = new HubConnectionBuilder()
            .WithUrl("http://localhost/ws", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(auth2.Token)!;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();

        Guid? onlineUserId = null;
        connection2.On<Guid>("UserOnline", id => {
            if (id != auth2.Id) onlineUserId = id;
        });
        await connection2.StartAsync();

        // Act - User 1 connects
        var connection1 = new HubConnectionBuilder()
            .WithUrl("http://localhost/ws", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(auth1.Token)!;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            })
            .Build();
        
        await connection1.StartAsync();

        // Assert
        for (int i = 0; i < 20 && onlineUserId == null; i++) await Task.Delay(100);
        onlineUserId.Should().Be(auth1.Id);

        await connection1.StopAsync();
        await connection2.StopAsync();
    }

    [Fact]
    public async Task TypingInChat_ShouldBroadcastToOthers()
    {
        // Arrange
        var client = Factory.CreateClient();
        var auth1 = await RegisterAndGetAuth(client, "typer", "typer@test.com");
        var auth2 = await RegisterAndGetAuth(client, "watcher", "watcher@test.com");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth1.Token);
        var chatResponse = await client.PostAsJsonAsync("/chats", new CreateChatRequest("Typing Chat", true, new List<Guid> { auth2.Id }));
        var chat = await chatResponse.Content.ReadFromJsonAsync<ChatResponse>();

        var conn1 = new HubConnectionBuilder()
            .WithUrl("http://localhost/ws", options => {
                options.AccessTokenProvider = () => Task.FromResult(auth1.Token)!;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            }).Build();

        var conn2 = new HubConnectionBuilder()
            .WithUrl("http://localhost/ws", options => {
                options.AccessTokenProvider = () => Task.FromResult(auth2.Token)!;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
            }).Build();

        string? typingData = null;
        conn2.On<object>("UserTyping", data => typingData = data.ToString());

        await conn1.StartAsync();
        await conn2.StartAsync();
        await conn1.InvokeAsync("JoinChat", chat!.Id.ToString());
        await conn2.InvokeAsync("JoinChat", chat.Id.ToString());

        // Act
        await conn1.InvokeAsync("SendTyping", chat.Id.ToString());

        // Assert
        for (int i = 0; i < 20 && typingData == null; i++) await Task.Delay(100);
        typingData.Should().NotBeNull();

        await conn1.StopAsync();
        await conn2.StopAsync();
    }
}
