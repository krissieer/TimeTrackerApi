﻿using Microsoft.AspNetCore.Authorization;
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
    /// Получить проекты, в которые включена активность
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    [HttpGet("{activityId}")]
    [Authorize]
    public async Task<IActionResult> GetActivityById(int activityId)
    {
        var activity = await activityService.GetActivityById(activityId);
        if (activity == null)
        {
            return NotFound($"Activity with ID {activityId} not found.");
        }
        var result =  new ActivityDto
        {
            id = activity.Id,
            name = activity.Name,
            userId = activity.UserId,
            activeFrom = activity.ActiveFrom,
            statusId = activity.StatusId
        };
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
        bool result = await activityService.AddActivity(dto.userId, dto.activityName);

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
    [HttpPut("{activityId}")]
    [Authorize]
    public async Task<ActionResult<bool>> UpdateActivityAsynс([FromBody] UpdateActivityDto dto, int activityId)
    {
        if (dto.updateName && string.IsNullOrWhiteSpace(dto.newName))
        {
            return BadRequest("New name must be provided when updating the name.");
        }

        var activity = await activityService.GetActivityById(activityId);
        if (activity == null)
        {
            return NotFound($"Activity with ID {activityId} not found.");
        }

        bool result = false;
        if (dto.updateName)
        {
            result = await activityService.UpdateActivityName(activityId, dto.newName);
        }
        if (activity.StatusId != 3 && dto.archived)
        {
            result = await activityService.ChangeStatus(activityId, 3);
        }
        if (activity.StatusId != 1 && !dto.archived)
        {
            result = await activityService.ChangeStatus(activityId, 1);
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
    public bool updateName { get; set; }
    public bool archived { get; set; }
    public string? newName { get; set; }
}
public class AddActivityRequest
{
    public int userId { get; set; }
    public string activityName { get; set; } = string.Empty;
}