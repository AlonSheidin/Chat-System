using System.Net.Http.Json;
using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;

namespace ChatSystem.IntegrationTests;

public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthTests(WebApplicationFactory<Program> factory)
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

    [Fact]
    public async Task Register_ShouldReturnAuthResponse()
    {
        // Arrange
        var client = _factory.CreateClient();
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
