using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ProjectUserService;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProjectUsersController : ControllerBase
{
    private readonly IProjectUserService projectUserService;

    public ProjectUsersController(IProjectUserService _projectUserService)
    {
        projectUserService = _projectUserService;
    }

    [HttpPost("add/{userId}/{projectId}")]
    [Authorize]
    public async Task<IActionResult> AddProjectUser(int userId, string projectId, bool isCreator)
    {
        var result = await projectUserService.AddProjectUser(userId,projectId,isCreator);
        if (result == null)
        {
            return BadRequest("Project does not exist or the user is already assigned to this project.");
        }
        return Ok(result);
    }

    [HttpDelete("delete/{userId}/{projectId}")]
    [Authorize]
    public async Task<IActionResult> DeleteProjectUser(int userId, string projectId)
    {
        var success = await projectUserService.DeleteProjectUser(userId, projectId);
        if (!success)
        {
            return NotFound("Record not found.");
        }
        return NoContent();
    }

    [HttpGet("users/{projectId}")]
    [Authorize]
    public async Task<IActionResult> GetUsersByProjectId(string projectId)
    {
        var users = await projectUserService.GetUsersByProjectId(projectId);
        if (users.Count == 0)
            return NotFound("Records not found");
        return Ok(users);
    }

    [HttpGet("projects/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetProjectsByUserId(int userId)
    {
        var projects = await projectUserService.GetProjectsByUserId(userId);
        if (projects.Count == 0)
            return NotFound("Records not found");
        return Ok(projects);
    }

    [HttpGet("projects/{userId}/{projectId}")]
    [Authorize]
    public async Task<IActionResult> GetIsCreator(int userId, string projectId)
    {
        var result = await projectUserService.IsCreator(userId, projectId);
        return Ok(result);
    }
}
