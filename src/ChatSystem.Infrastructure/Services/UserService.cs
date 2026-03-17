using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.Interfaces.Repositories;
using ChatSystem.Application.Interfaces.Services;

namespace ChatSystem.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return new UserResponse(user.Id, user.Username, user.Email);
    }

    public async Task<IEnumerable<UserResponse>> SearchAsync(string query)
    {
        var users = await _userRepository.SearchAsync(query);
        return users.Select(u => new UserResponse(u.Id, u.Username, u.Email));
    }
}
