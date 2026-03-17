namespace ChatSystem.Application.DTOs.Auth;

public record UserResponse(
    Guid Id,
    string Username,
    string Email
);
