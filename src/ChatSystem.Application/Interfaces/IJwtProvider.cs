using ChatSystem.Domain.Entities;

namespace ChatSystem.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}
