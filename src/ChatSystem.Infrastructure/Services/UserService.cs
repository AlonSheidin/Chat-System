using ChatSystem.Application.DTOs.Auth;
using ChatSystem.Application.Interfaces.Repositories;
using ChatSystem.Application.Interfaces.Services;

namespace ChatSystem.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPresenceService _presenceService;

    public UserService(IUserRepository userRepository, IPresenceService presenceService)
    {
        _userRepository = userRepository;
        _presenceService = presenceService;
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        var status = await _presenceService.GetUserStatusAsync(id);
        var lastSeen = await _presenceService.GetLastSeenAsync(id);

        return new UserResponse(user.Id, user.Username, user.Email, status.ToString(), lastSeen);
    }

    public async Task<IEnumerable<UserResponse>> SearchAsync(string query)
    {
        var users = await _userRepository.SearchAsync(query);
        var response = new List<UserResponse>();

        foreach (var user in users)
        {
            var status = await _presenceService.GetUserStatusAsync(user.Id);
            var lastSeen = await _presenceService.GetLastSeenAsync(user.Id);
            response.Add(new UserResponse(user.Id, user.Username, user.Email, status.ToString(), lastSeen));
        }

        return response;
    }
}
