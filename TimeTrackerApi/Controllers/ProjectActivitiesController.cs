using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost("add")]
    [Authorize]
    public async Task<IActionResult> AddProjectActivity(int activityId, string projectId)
    {
        var result = await projectActivityService.AddProjectActivity(activityId, projectId);
        if (result == null)
            return Conflict("Activity already exists for this project or project does not exist");
        return Ok(result);
    }

    [HttpDelete("delete")]
    [Authorize]
    public async Task<IActionResult> DeleteProjectActivity(int activityId, string projectId)
    {
        var result = await projectActivityService.DeleteProjectActivity(activityId, projectId);
        if (!result)
            return NotFound("Record not found");
        return Ok(result);
    }

    [HttpGet("activities/{projectId}")]
    [Authorize]
    public async Task<IActionResult> GetActivitiesByProjectId(string projectId)
    {
        var activities = await projectActivityService.GetActivitiesByProjectId(projectId);
        if (activities.Count == 0)
            return NotFound("Records not found");
            //return Ok(new List<Activity>());???
        return Ok(activities);
    }

    [HttpGet("projects/{activityId}")]
    [Authorize]
    public async Task<IActionResult> GetProjectsByActivityId(int activityId)
    {
        var projects = await projectActivityService.GetProjectsByActivityId(activityId);
        if (projects.Count == 0)
            return NotFound("Records not found");
        return Ok(projects);
    }
}
