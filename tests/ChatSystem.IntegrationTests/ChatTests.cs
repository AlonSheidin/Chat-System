using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.DTOs.Chat;
using ChatSystem.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatSystem.IntegrationTests;

public class ChatTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ChatTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = "super_secret_key_that_is_long_enough_for_hmac_sha256",
                    ["Jwt:Issuer"] = "ChatSystem",
                    ["Jwt:Audience"] = "ChatSystemClients",
                    ["Jwt:ExpiryMinutes"] = "60",
                    ["UseInMemoryDatabase"] = "true"
                });
            });
        });
    }

    private async Task<AuthResponse> RegisterAndGetAuth(HttpClient client, string username, string email)
    {
        var request = new RegisterRequest(username, email, "Password123!");
        var response = await client.PostAsJsonAsync("/auth/register", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    [Fact]
    public async Task CreateChat_ShouldReturnChat()
    {
        // Arrange
        var client = _factory.CreateClient();
        var auth = await RegisterAndGetAuth(client, "chatcreator", "creator@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var request = new CreateChatRequest("Test Chat", true, new List<Guid>());

        // Act
        var response = await client.PostAsJsonAsync("/chats", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var chat = await response.Content.ReadFromJsonAsync<ChatResponse>();
        chat.Should().NotBeNull();
        chat!.Name.Should().Be("Test Chat");
    }

    [Fact]
    public async Task SendMessage_ShouldReturnMessage()
    {
        // Arrange
        var client = _factory.CreateClient();
        var auth = await RegisterAndGetAuth(client, "msgsender", "sender@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var chatRequest = new CreateChatRequest("Msg Chat", false, new List<Guid>());
        var chatResponse = await client.PostAsJsonAsync("/chats", chatRequest);
        var chat = await chatResponse.Content.ReadFromJsonAsync<ChatResponse>();

        var msgRequest = new SendMessageRequest("Hello Test!");

        // Act
        var response = await client.PostAsJsonAsync($"/chats/{chat!.Id}/messages", msgRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        var msg = await response.Content.ReadFromJsonAsync<MessageResponse>();
        msg.Should().NotBeNull();
        msg!.Content.Should().Be("Hello Test!");
    }

    [Fact]
    public async Task AddMember_ShouldIncreaseMemberCount()
    {
        // Arrange
        var client = _factory.CreateClient();
        var admin = await RegisterAndGetAuth(client, "admin", "admin@test.com");
        var newUser = await RegisterAndGetAuth(client, "newbie", "newbie@test.com");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.Token);
        var chatResponse = await client.PostAsJsonAsync("/chats", new CreateChatRequest("Group Chat", true, new List<Guid>()));
        var chat = await chatResponse.Content.ReadFromJsonAsync<ChatResponse>();

        // Act
        var addResponse = await client.PostAsJsonAsync($"/chats/{chat!.Id}/members", new AddMemberRequest(newUser.Id));

        // Assert
        addResponse.EnsureSuccessStatusCode();
        
        var updatedChatResponse = await client.GetAsync($"/chats/{chat.Id}");
        var updatedChat = await updatedChatResponse.Content.ReadFromJsonAsync<ChatResponse>();
        updatedChat!.MemberIds.Should().Contain(newUser.Id);
    }

    [Fact]
    public async Task GetMyChats_ShouldReturnChats()
    {
        // Arrange
        var client = _factory.CreateClient();
        var auth = await RegisterAndGetAuth(client, "meuser", "me@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var chatRequest = new CreateChatRequest("My Chat", true, new List<Guid>());
        await client.PostAsJsonAsync("/chats", chatRequest);

        // Act
        var response = await client.GetAsync("/users/me/chats");

        // Assert
        response.EnsureSuccessStatusCode();
        var chats = await response.Content.ReadFromJsonAsync<IEnumerable<ChatResponse>>();
        chats.Should().NotBeNull();
        chats.Should().Contain(c => c.Name == "My Chat");
    }
}
