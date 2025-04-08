using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;
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
    public async Task<ActionResult> GetUsers()
    {
        var users = await userService.GetUsers() ?? new List<User>();
        if (!users.Any())
        {
            return Ok(new List<UserDto>());
        }
        var result = users.Select(a => new UserDto
        {
            id = a.Id,
            chatId = a.ChatId,
            name = a.Name
        });

        return Ok(result);
    }

    /// <summary>
    /// Получить данные об одном пользователе
    /// </summary>
    /// <returns></returns>
    [HttpGet("{userId}")]
    public async Task<ActionResult> GetUserById(int userId)
    {
        var user = await userService.GetUserById(userId);
        if (user is null)
        {
            return NotFound($"User with ID {userId} not found.");
        }
        var result = new UserDto
        {
            id = user.Id,
            chatId = user.ChatId,
            name = user.Name
        };

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
    public async Task<ActionResult> GetActivities(int userId, [FromQuery] bool? onlyArchived, [FromQuery] bool? onlyActive)
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
            id = a.Id,
            name = a.Name,
            activeFrom = a.ActiveFrom,
            userId = a.UserId,
            statusId = a.StatusId
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
    public async Task<ActionResult> GetProjectsByUserId(int userId)
    {
        var projects = await projectUserService.GetProjectsByUserId(userId);
        if (!projects.Any())
        {
            return Ok(new List<ProjectUserDto>());
        }
        var result = projects.Select(a => new ProjectUserDto
        {
            id = a.Id,
            userId = a.UserId,
            projectId = a.ProjectId,
            isCreator = a.Creator
        });

        return Ok(result);
    }

    /// <summary>
    /// Регистрация/вход
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> RegistrtationLogin([FromBody] AuthDto dto)
    {
        if (!ModelState.IsValid) // имя и пароль не пустые
        {
            return BadRequest(ModelState);
        }
        string token;
        if ( dto.isNewUser )
        {
            token = await userService.Registration(dto.name, dto.password, dto.chatId);
            if (string.IsNullOrEmpty(token))
                return Conflict("This username is already in use");
        }
        else
        {
            token = await userService.Login(dto.name, dto.password);
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Username or password is wrong");
        }

        return Ok(new { Token = token });
    }

    /// <summary>
    /// Подключиться к проекту
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <param name="isCreator"></param>
    /// <returns></returns>
    [HttpPost("project")]
    [Authorize]
    public async Task<ActionResult> ConnectToProject([FromBody] ConnectToProjectDto dto)
    {
        var USER = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(USER))
            return Unauthorized("User not authenticated.");
        int user_Id = int.Parse(USER);

        var user = await projectUserService.ConnectToProject(user_Id, dto.accessKey);
        if (user == null)
        {
            return Conflict("Project does not exist or the user is already assigned to this project.");
        }
        return Ok(new ProjectUserDto
        {
            id = user.Id,
            userId = user.UserId,
            projectId = user.ProjectId,
            isCreator = user.Creator,
        });
    }

    [HttpPut("{userId}")]
    [Authorize]
    public async Task<ActionResult> EditUser([FromBody] EditUserDto dto, int userId)
    {
        var user = await userService.GetUserById(userId);
        if (user == null)
            return NotFound($"User with ID {userId} not found.");
        bool updated = false;
        if (dto.updateName && dto.updatePassword)
            updated = await userService.UpdateUser(userId, dto.userName, dto.password);
        else if (dto.updateName && !dto.updatePassword)
            updated = await userService.UpdateUser(userId, dto.userName);
        else if (!dto.updateName && dto.updatePassword)
            updated = await userService.UpdateUser(userId, null, dto.password);
        if (!updated)
            StatusCode(500, "Failed to edit user due to server error.");

        var result = new UserDto
        {
            id = user.Id,
            chatId = user.ChatId,
            name = user.Name
        };
        return Ok(result);
    }

    [HttpDelete("{userId}")]
    [Authorize]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var success = await userService.DeleteUser(userId);
        if (!success)
            StatusCode(500, "Failed to delete user due to server error.");
        return NoContent();
    }
}

public class AuthDto
{
    [Required]
    public bool isNewUser { get; set; }
    [Required]
    public string name { get; set; } = string.Empty;
    [Required]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string password { get; set; } = string.Empty;
    public int chatId { get; set; } = 0;
}

public class UserDto
{
    public int id { get; set; }
    public long chatId { get; set; }
    public string name { get; set; } = string.Empty;
}

public class ActivityDto
{
    public int id { get; set; }
    public string name { get; set; }
    public DateTime activeFrom { get; set; }
    public int userId { get; set; }
    public int statusId { get; set; }
}

public class EditUserDto
{
    public bool updateName { get; set; }
    public bool updatePassword { get; set; }
    public string? userName { get; set; } = string.Empty;
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string? password { get; set; } = string.Empty;
}

public class ConnectToProjectDto
{
    public string accessKey { get; set; }
}
