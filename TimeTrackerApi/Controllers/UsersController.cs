using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;
using TimeTrackerApi.Services.ProjectService;
using TimeTrackerApi.Services.ProjectUserService;
using TimeTrackerApi.Services.UserService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService userService;
    private readonly IActivityService activityService;
    private readonly IProjectUserService projectUserService;

    public UsersController(IUserService _userService, IActivityService _activityService, IProjectUserService _projectUserService)
    {
        userService = _userService;
        activityService = _activityService;
        projectUserService = _projectUserService;
    }

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<User>> GetUsers()
    {
        var users = await userService.GetUsers() ?? new List<User>();
        if (!users.Any())
        {
            return Ok(new List<UserDto>());
        }
        var result = users.Select(a => new UserDto
        {
            Id = a.Id,
            ChatId = a.ChatId,
            Name = a.Name
        });

        return Ok(result);
    }

    /// <summary>
    /// Получить активности пользователя
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="onlyArchived"></param>
    /// <param name="onlyActive"></param>
    /// <returns></returns>
    [HttpGet("{userId}/activities")]
    [Authorize]
    public async Task<IActionResult> GetActivities(int userId, [FromQuery] bool? onlyArchived, [FromQuery] bool? onlyActive)
    {
        if (onlyArchived == true && onlyActive == true)
        {
            return BadRequest("Cannot request both archived and active activities at the same time.");
        }
        var activities = await activityService.GetActivities(userId, onlyActive ?? false, onlyArchived ?? false);
        if (!activities.Any())
        {
            return Ok(new List<ActivityDto>());
        }
        var result = activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            Name = a.Name,
            ActiveFrom = a.ActiveFrom,
            UserId = a.UserId,
            StatusId = a.StatusId
        });

        return Ok(result);
    }

    /// <summary>
    /// Получить проекты, в которых участвует пользователь
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("{userId}/projects")]
    [Authorize]
    public async Task<IActionResult> GetProjectsByUserId(int userId)
    {
        var projects = await projectUserService.GetProjectsByUserId(userId);
        if (!projects.Any())
        {
            return Ok(new List<ProjectDto>());
        }
        var result = projects.Select(a => new ProjectDto
        {
            Id = a.Id,
            UserId = a.UserId,
            ProjectId = a.ProjectId,
            Creator = a.Creator
        });

        return Ok(result);
    }

    /// <summary>
    /// Регистрация/вход
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> RegistrtationLogin([FromBody] AuthRequestDto dto)
    {
        if (!ModelState.IsValid) // имя и пароль не пустые
        {
            return BadRequest(ModelState);
        }

        string token;

        if ( dto.IsNewUser )
        {
            token = await userService.Registration(dto.Name, dto.Password, dto.ChatId);
            if (string.IsNullOrEmpty(token))
                return Conflict("This username is already in use");
        }
        else
        {
            token = await userService.Login(dto.Name, dto.Password);
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Username or password is wrong");
        }

        return Ok(new { Token = token });
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> EditUser([FromBody] EditUserDto dto)
    {
        var user = await userService.GetUserById(dto.userId);
        if (user == null)
            return NotFound($"User with ID {dto.userId} not found.");
        bool updated = false;
        if (dto.updateName && dto.updatePassword)
            updated = await userService.UpdateUser(dto.userId, dto.userName, dto.password);
        else if (dto.updateName && !dto.updatePassword)
            updated = await userService.UpdateUser(dto.userId, dto.userName);
        else if (!dto.updateName && dto.updatePassword)
            updated = await userService.UpdateUser(dto.userId, null, dto.password);
        if (!updated)
            StatusCode(500, "Failed to edit user due to server error.");

        var result = new UserDto
        {
            Id = user.Id,
            ChatId = user.ChatId,
            Name = user.Name
        };
        return Ok(result);

    }

    [HttpDelete("{userId}")]
    [Authorize]
    public async Task<ActionResult> DeleteUser(int userId)
    {
        var success = await userService.DeleteUser(userId);
        if (!success)
            StatusCode(500, "Failed to delete user due to server error.");
        return NoContent();
    }
}

public class AuthRequestDto
{
    [Required]
    public bool IsNewUser { get; set; }
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;
    public int ChatId { get; set; } = 0;
}

public class UserDto
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ActivityDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime ActiveFrom { get; set; }
    public int UserId { get; set; }
    public int StatusId { get; set; }
}

public class ProjectDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public bool Creator { get; set; }
}

public class EditUserDto
{
    public int userId { get; set; }
    public bool updateName { get; set; }
    public bool updatePassword { get; set; }
    public string? userName { get; set; } = string.Empty;
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string? password { get; set; } = string.Empty;
}
