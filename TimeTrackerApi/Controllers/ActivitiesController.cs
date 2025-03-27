using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;
using TimeTrackerApi.Services.ProjectActivityService;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService activityService;
    private readonly IProjectActivityService projectActivityService;

    public ActivitiesController(IActivityService _activityService, IProjectActivityService _projectActivityService)
    {
        activityService = _activityService;
        projectActivityService = _projectActivityService;
    }

    /// <summary>
    /// Получить статус активности
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    [HttpGet("{activityId}/status")]
    [Authorize]
    public async Task<IActionResult> GetStatusAsync(int activityId)
    {
        var status = await activityService.GetStatusById(activityId);
        if (status == 0)
            return NotFound($"Activity with ID {activityId} not found");
        return Ok(status);
    }

    /// <summary>
    /// Получить проекты, в которые включена активность
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    [HttpGet("{activityId}/projects")]
    [Authorize]
    public async Task<IActionResult> GetProjectsByActivityId(int activityId)
    {
        var activityExists = await activityService.GetActivityById(activityId);
        if (activityExists == null)
        {
            return NotFound($"Activity with ID {activityId} not found.");
        }
        var projects = await projectActivityService.GetProjectsByActivityId(activityId);
        if (!projects.Any())
        {
            return NotFound("No projects found for this activity.");
        }
        var result = projects.Select(a => new
        {
            a.Id,
            a.ProjectId,
            a.ActivityId
        });
        return Ok(result);
    }

    /// <summary>
    /// Добавить активность
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<bool>> AddActivitiesAsync([FromBody] AddActivityRequest dto)
    {
        bool result = await activityService.AddActivity(dto.UserId, dto.ActivityName);

        if (!result)
            return Conflict("Activity with the same activity name already exists");
       
        return Ok(result);
    }

    /// <summary>
    /// Добавить дефолтные активности
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpPost("add-defualt")]
    [Authorize]
    public async Task<ActionResult<bool>> AddDefaultActivitiesAsync(int userId)
    {
        bool result = await activityService.AddDefaultActivities(userId);
        if (!result)
            return BadRequest("Default activities could not be added");
        return Ok(result);
    }

    /// <summary>
    /// Обновить активность: изменить имя или изменить статус
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut]
    [Authorize]
    public async Task<ActionResult<bool>> UpdateActivityAsynс([FromBody] UpdateActivityDto dto)
    {
        if (dto.UpdateName && string.IsNullOrWhiteSpace(dto.NewName))
        {
            return BadRequest("New name must be provided when updating the name.");
        }

        var activity = await activityService.GetActivityById(dto.ActivityId);
        if (activity == null)
        {
            return NotFound($"Activity with ID {dto.ActivityId} not found.");
        }

        bool result = false;
        if (dto.UpdateName)
        {
            result = await activityService.UpdateActivityName(dto.ActivityId, dto.NewName);
        }
        if (activity.StatusId != 3 && dto.Archived)
        {
            result = await activityService.ChangeStatus(dto.ActivityId, 3);
        }
        if (activity.StatusId != 1 && !dto.Archived)
        {
            result = await activityService.ChangeStatus(dto.ActivityId, 1);
        }

        if (!result)
            return StatusCode(500, "Failed to update activity due to server error.");

        return Ok(result);
    }

    /// <summary>
    /// Удалить активность
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    [HttpDelete("{activityId}")]
    [Authorize]
    public async Task<ActionResult> DeleteActivityAsync(int activityId)
    {
        var activity = await activityService.GetActivityById(activityId);
        if (activity == null)
        {
            return NotFound($"Activity with ID {activityId} not found.");
        }

        var result = await activityService.DeleteActivity(activityId);
        if (!result)
            return StatusCode(500, "Failed to delete activity due to server error.");
        return NoContent();
    }
}

public class UpdateActivityDto
{
    public int ActivityId { get; set; }
    public bool UpdateName { get; set; }
    public bool Archived { get; set; }
    public string? NewName { get; set; }
}
public class AddActivityRequest
{
    public int UserId { get; set; }
    public string ActivityName { get; set; } = string.Empty;
}