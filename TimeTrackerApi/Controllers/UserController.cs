using Microsoft.AspNetCore.Mvc;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.UserService;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService userService;

    public UserController(IUserService _userService)
    {
        Console.WriteLine("UserController initialized");
        userService = _userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsersNumberAsync()
    {
        var result = await userService.GetUsersNumber();
        return Ok(new { UserCount = result });
    }

    [HttpGet("by-userId/{userId}")]
    public async Task<ActionResult> GetUserByIdAsync(int userId)
    {
        var result = await userService.GetUserById(userId);
        if (result is null)
            return NotFound("Record not found");
        return Ok(result);
    }

    [HttpGet("by-chatId/{chatId}")]
    public async Task<ActionResult<User>> GetUserByChatIdAsync(long chatId)
    {
        var result = await userService.GetUserByChatId(chatId);
        if (result is null)
            return NotFound("Record not found");
        return Ok(result);
    }

    [HttpPost("registration")]
    public IActionResult Registrtation(string name, string password, int chatId = 0)
    {
        var token = userService.Registration(name, password, chatId);

        if (token.Equals(string.Empty))
            return BadRequest("This username is already in use");
        else if (token is null)
            return BadRequest();

        var response = new
        {
            access_token = token,
            userName = name,
            chatId = chatId,        
        };

        return Ok(response);
    }

    [HttpPost("login")]
    public IActionResult Login(string name, string password)
    {
        var token = userService.Login(name, password);

        if (token.Equals(string.Empty))
            return BadRequest("Username or password is wrong");

        var response = new
        {
            access_token = token,
            userName = name,
        };

        return Ok(response);
    }
}
