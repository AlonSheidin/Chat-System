using ChatSystem.Application.DTOs.Auth;

namespace ChatSystem.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
