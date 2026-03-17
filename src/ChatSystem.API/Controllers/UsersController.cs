using System.Security.Claims;
using ChatSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IChatService _chatService;

    public UsersController(IUserService userService, IChatService chatService)
    {
        _userService = userService;
        _chatService = chatService;
    }

    private Guid GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim == null) throw new UnauthorizedAccessException();
        return Guid.Parse(idClaim.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return BadRequest("Query is required.");
        var users = await _userService.SearchAsync(query);
        return Ok(users);
    }

    [HttpGet("me/chats")]
    public async Task<IActionResult> GetMyChats()
    {
        var userId = GetUserId();
        var chats = await _chatService.GetUserChatsAsync(userId);
        return Ok(chats);
    }
}
