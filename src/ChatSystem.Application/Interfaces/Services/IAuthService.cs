using ChatSystem.Application.DTOs.Auth;

namespace ChatSystem.Application.Interfaces.Services;

/// <summary>
/// Service for user authentication and account management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user and returns an authentication token.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user and returns a JWT token if successful.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
