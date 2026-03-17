using ChatSystem.Application.DTOs.Auth;

namespace ChatSystem.Application.Interfaces.Services;

public interface IUserService
{
    Task<UserResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<UserResponse>> SearchAsync(string query);
}
