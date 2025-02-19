using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;
using TimeTrackerApi.Services.ProjectUserService;
using TimeTrackerApi.Services.UserService;

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

    [HttpGet]
    public async Task<ActionResult<User>> GetUsers()
    {
        var result = await userService.GetUsers();
        if (result is null)
            return NotFound("Record not found");
        return Ok(result);
    }

    [HttpGet("{userId}/activities")]
    [Authorize]
    public async Task<IActionResult> GetActivities(bool onlyArchived, bool onlyActive, int userId)
    {
        List<Activity> activities = null;
        //все активности
        if (!onlyArchived && !onlyActive)
        {
            activities = await activityService.GetActivities(userId, false, false);
        }
        //только активные
        else if (onlyActive)
        {
            activities = await activityService.GetActivities(userId);
        }
        //только архивированные
        else if (onlyArchived)
        {
            activities = await activityService.GetActivities(userId, false, true);
        }
        if (activities.Count == 0)
            return NotFound("Records not found");
        return Ok(activities);
    }

    [HttpGet("{userId}/projects")]
    [Authorize]
    public async Task<IActionResult> GetProjectsByUserId(int userId)
    {
        var projects = await projectUserService.GetProjectsByUserId(userId);
        if (projects.Count == 0)
            return NotFound("Records not found");
        return Ok(projects);
    }

    [HttpPost]
    public async Task<IActionResult> Registrtation(bool isNewUser, string name, string password, int chatId = 0)
    {
        string token;
        if ( isNewUser )
        {
            token = await userService.Registration(name, password, chatId);
            if (string.IsNullOrEmpty(token))
                return NotFound("This username is already in use");
        }
        else
        {
            token = await userService.Login(name, password);
            if (string.IsNullOrEmpty(token))
                return NotFound("Username or password is wrong");
        }

        if (token is null)
            return BadRequest();

        return Ok(token);
    }
}
