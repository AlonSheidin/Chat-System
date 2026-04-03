using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChatSystem.Application.DTOs.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChatSystem.IntegrationTests;

public class UserTests : TestBase
{
    public UserTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    private async Task<string> RegisterAndGetToken(HttpClient client, string username, string email)
    {
        var request = new RegisterRequest(username, email, "Password123!");
        var response = await client.PostAsJsonAsync("/auth/register", request);
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }

    [Fact]
    public async Task Search_ShouldReturnUsers()
    {
        // Arrange
        var client = Factory.CreateClient();
        var token = await RegisterAndGetToken(client, "searchuser", "search@example.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/users/search?query=searchuser");

        // Assert
        response.EnsureSuccessStatusCode();
        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();
        users.Should().NotBeNull();
        users.Should().Contain(u => u.Username == "searchuser");
    }

    [Fact]
    public async Task GetById_ShouldReturnUser()
    {
        // Arrange
        var client = Factory.CreateClient();
        var request = new RegisterRequest("getuser", "get@example.com", "Password123!");
        var regResponse = await client.PostAsJsonAsync("/auth/register", request);
        var authResponse = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse!.Token);

        // Act
        var response = await client.GetAsync($"/users/{authResponse.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("getuser");
    }
}
