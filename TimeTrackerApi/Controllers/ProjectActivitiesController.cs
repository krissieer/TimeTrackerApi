using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;
using TimeTrackerApi.Services.ProjectActivityService;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectActivitiesController : ControllerBase
{
    private readonly IProjectActivityService projectActivityService;

    public ProjectActivitiesController(IProjectActivityService _projectActivityService)
    {
        projectActivityService = _projectActivityService;
    }

    /// <summary>
    /// Получить активности опредленного проекта
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetActivities(string projectId)
    {
        var activities = await projectActivityService.GetActivitiesByProjectId(projectId);
        if (!activities.Any() || activities == null)
        {
            return NotFound("No activities found for this project ");
        }
        var result = activities.Select(a => new ProjectActivityDto
        {
            Id = a.Id,
            ActivityId = a.ActivityId,
            ProjectId = a.ProjectId
        });
        return Ok(result);
    }

    //[HttpGet("projects/{activityId}")]
    //[Authorize]
    //public async Task<IActionResult> GetProjectsByActivityId(int activityId)
    //{
    //    var projects = await projectActivityService.GetProjectsByActivityId(activityId);
    //    if (projects.Count == 0)
    //        return NotFound("Records not found");
    //    return Ok(projects);
    //}

    /// <summary>
    /// Добавить Активность в Проект
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddProjectActivity(int activityId, string projectId)
    {
        var result = await projectActivityService.AddProjectActivity(activityId, projectId);
        if (result == null)
            return Conflict("Activity already exists for this project or project does not exist");

        var response = new ProjectActivityDto
        {
            Id = result.Id,
            ActivityId = result.ActivityId,
            ProjectId = result.ProjectId
        };

        return Ok(response);
    }

    [HttpDelete("{projectId}/{activityId}")]
    [Authorize]
    public async Task<IActionResult> DeleteProjectActivity(string projectId, int activityId)
    {
        var result = await projectActivityService.DeleteProjectActivity(activityId, projectId);
        if (!result)
            return StatusCode(500, "Failed to delete activity from project due to server error.");
        return NoContent();
    }
}

public class ProjectActivityDto
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public string ProjectId { get; set; }
}
