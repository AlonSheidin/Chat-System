using System.Security.Claims;
using ChatSystem.Application.DTOs.Chat;
using ChatSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatSystem.API.Controllers;

[Authorize]
[ApiController]
[Route("chats")]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatsController(IChatService chatService)
    {
        _chatService = chatService;
    }

    private Guid GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim == null) throw new UnauthorizedAccessException();
        return Guid.Parse(idClaim.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat(CreateChatRequest request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _chatService.CreateChatAsync(userId, request);
            return CreatedAtAction(nameof(GetChat), new { chatId = response.Id }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{chatId}")]
    public async Task<IActionResult> GetChat(Guid chatId)
    {
        try
        {
            var userId = GetUserId();
            var response = await _chatService.GetChatAsync(userId, chatId);
            if (response == null) return NotFound();
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{chatId}/members")]
    public async Task<IActionResult> AddMember(Guid chatId, [FromBody] AddMemberRequest request)
    {
        try
        {
            var userId = GetUserId();
            await _chatService.AddMemberToChatAsync(userId, chatId, request.UserId);
            return Ok(new { message = "User added successfully." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{chatId}/messages")]
    public async Task<IActionResult> SendMessage(Guid chatId, [FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = GetUserId();
            var response = await _chatService.SendMessageAsync(userId, chatId, request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(Guid chatId)
    {
        try
        {
            var userId = GetUserId();
            var response = await _chatService.GetMessagesAsync(userId, chatId);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
