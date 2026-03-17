using System.ComponentModel.DataAnnotations;

namespace ChatSystem.Application.DTOs.Auth;

public record RegisterRequest(
    [Required] string Username,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(
    Guid Id,
    string Username,
    string Email,
    string Token
);
