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

    /// <summary>
    /// Получить пользователй проекта
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProjectUsers(string projectId)
    {
        var users = await projectUserService.GetUsersByProjectId(projectId);

        if (!users.Any())
        {
            return NotFound($"No users found for project with ID {projectId}");
        }
        var result = users.Select(a => new ProjectUserDto
        {
            Id = a.Id,
            ProjectId = a.ProjectId,
            UserId = a.UserId,
            IsCreator = a.Creator
        });

        return Ok(result);
    }

    /// <summary>
    /// Является ли пользователь создателем проекта
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpGet("{projectId}/{userId}/role")]
    [Authorize]
    public async Task<ActionResult<bool>> GetIsCreator(int userId, string projectId)
    {
        var result = await projectUserService.IsCreator(userId, projectId);
        return Ok(result);
    }

    /// <summary>
    /// Добавить поьзователя в проект
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <param name="isCreator"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddProjectUser([FromBody] ProjectUserDto dto)
    {
        var result = await projectUserService.AddProjectUser(dto.UserId,dto.ProjectId,dto.IsCreator);
        if (result == null)
        {
            return Conflict("Project does not exist or the user is already assigned to this project.");
        }
        return Ok(new ProjectUserDto
        {
            Id = result.Id,
            UserId = result.UserId,
            ProjectId = result.ProjectId,
            IsCreator = result.Creator
        });
    }

    /// <summary>
    /// Удалить пользователя из проекта
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    [HttpDelete("{projectId}/{userId}")]
    [Authorize]
    public async Task<ActionResult<bool>> DeleteProjectUser(string projectId, int userId)
    {
        var success = await projectUserService.DeleteProjectUser(userId, projectId);
        if (!success)
            return StatusCode(500, "Failed to delete activity from project due to server error.");
        return NoContent();
    }
}

public class ProjectUserDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ProjectId { get; set; }
    public bool IsCreator { get; set; }
}
