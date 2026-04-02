using System.Net.Http.Json;
using ChatSystem.Application.DTOs.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChatSystem.IntegrationTests;

public class AuthTests : TestBase
{
    public AuthTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_ShouldReturnAuthResponse()
    {
        // Arrange
        var client = Factory.CreateClient();
        var request = new RegisterRequest("testuser", "test@example.com", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/auth/register", request);

        // Assert
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Request failed with status {response.StatusCode} and body: {error}");
        }
        response.EnsureSuccessStatusCode();
        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        authResponse.Should().NotBeNull();
        authResponse!.Username.Should().Be("testuser");
        authResponse.Token.Should().NotBeNullOrEmpty();
    }
}
