using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TimeTrackerApi.Services.ActivityService;
using TimeTrackerApi.Services.ProjectUserService;
using TimeTrackerApi.Services.UserService;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : Controller
{
    private readonly IUserService userService;

    public AuthController(IUserService _userService)
    {
        userService = _userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        string token;
        try
        {
            token = await userService.Login(dto.name, dto.password, dto.chatId);
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Username or password is wrong");
            return Ok(new { Token = token });
        }
        catch (Exception ex) { return BadRequest(ex); }
    }
}
public class AuthDto
{
    [Required]
    public string name { get; set; } = string.Empty;
    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string password { get; set; } = string.Empty;
    public int chatId { get; set; } = 0;
}