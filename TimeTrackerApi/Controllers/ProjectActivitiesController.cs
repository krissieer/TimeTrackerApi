using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;
using TimeTrackerApi.Services.ProjectActivityService;
using TimeTrackerApi.Services.ProjectUserService;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectActivitiesController : ControllerBase
{
    private readonly IProjectActivityService projectActivityService;
    private readonly IProjectUserService projectUserService;

    public ProjectActivitiesController(IProjectActivityService _projectActivityService, IProjectUserService _projectUserService)
    {
        projectActivityService = _projectActivityService;
        projectUserService = _projectUserService;
    }

    /// <summary>
    /// Получить активности опредленного проекта
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetActivities(int projectId)
    {
        var activities = await projectActivityService.GetActivitiesByProjectId(projectId);
        if (!activities.Any() || activities == null)
        {
            return NotFound("No activities found for this project ");
        }
        var result = activities.Select(a => new ProjectActivityDto
        {
            id = a.Id,
            activityId = a.ActivityId,
            projectId = a.ProjectId
        });
        return Ok(result);
    }

    /// <summary>
    /// Добавить Активность в Проект
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddProjectActivity(int activityId, int projectId)
    {
        var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userID))
            return Unauthorized("User not authenticated.");
        int userId = int.Parse(userID);

        var isCreator = await projectUserService.IsCreator(userId, projectId);
        if (!isCreator)
            return Conflict("You don't have access to edit this project");

        var result = await projectActivityService.AddProjectActivity(activityId, projectId);
        if (result == null)
            return Conflict("Activity already exists for this project or project does not exist");

        var response = new ProjectActivityDto
        {
            id = result.Id,
            activityId = result.ActivityId,
            projectId = result.ProjectId
        };
        return Ok(response);
    }

    [HttpDelete("{projectId}/{activityId}")]
    [Authorize]
    public async Task<IActionResult> DeleteProjectActivity(int projectId, int activityId)
    {
        var userID = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userID))
            return Unauthorized("User not authenticated.");
        int userId = int.Parse(userID);

        var isCreator = await projectUserService.IsCreator(userId, projectId);
        if (!isCreator)
            return Conflict("You don't have access to delete this activity.");

        var result = await projectActivityService.DeleteProjectActivity(activityId, projectId);
        if (!result)
            return StatusCode(500, "Failed to delete activity from project due to server error.");
        return NoContent();
    }
    //Только создатель прокта может добавлять и удалять зададчи в проект
}

public class ProjectActivityDto
{
    public int id { get; set; }
    public int activityId { get; set; }
    public int projectId { get; set; }
}
