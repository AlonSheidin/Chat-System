using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.Interfaces;
using ChatSystem.Application.Interfaces.Repositories;
using ChatSystem.Application.Interfaces.Services;
using ChatSystem.Domain.Entities;
using BCrypt.Net;

namespace ChatSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(IUserRepository userRepository, IJwtProvider jwtProvider)
    {
        _userRepository = userRepository;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new Exception("User with this email already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = _jwtProvider.GenerateToken(user);

        return new AuthResponse(user.Id, user.Username, user.Email, token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid email or password.");
        }

        var token = _jwtProvider.GenerateToken(user);

        return new AuthResponse(user.Id, user.Username, user.Email, token);
    }
}
