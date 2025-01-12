using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivityController : ControllerBase
{
    private readonly IActivityService activityService;

    public ActivityController(IActivityService _activityService)
    {
        activityService = _activityService;
    }

    [HttpGet("activeActivities/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetActivityListByIdAsync(int userId)
    {
        var activities = await activityService.GetActivities(userId);
        if (activities.Count ==0)
            return NotFound("Records not found");
        return Ok(activities);
    }
    
    [HttpGet("allActivities/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetAllActivityListByIdAsync(int userId)
    {
        var activities = await activityService.GetActivities(userId, false);
        if (activities.Count == 0)
            return NotFound("Records not found");
        return Ok(activities);
    }

    [HttpGet("archivedActivities/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetArchivedActivityListByIdAsync(int userId)
    {
        var activities = await activityService.GetActivities(userId, false, true);
        if (activities.Count == 0)
            return NotFound("Records not found");
        return Ok(activities);
    }

    [HttpPost("default/{userId}")]
    [Authorize]
    public async Task<ActionResult<bool>> AddDefaultActivitiesAsync(int userId)
    {
        var result = await activityService.AddDefaultActivities(userId);
        if (!result)
            return BadRequest("Failed to add default activities");
        return Ok(result);
    }

    [HttpPost("add/{userId}/{activityName}")]
    [Authorize]
    public async Task<ActionResult<Activity>> AddActivityAsync(int userId, string activityName)
    {
        var result = await activityService.AddActivity(userId, activityName);
        if (result is null)
            return BadRequest("Activity with the same activityName already exists.");
        return Ok(result);
    }

    [HttpPut("update/{activityId}/{newName}")]
    [Authorize]
    public async Task<ActionResult<bool>> UpdateActivityNameAsynс(int activityId, string newName)
    {
        var result = await activityService.UpdateActivityName(activityId, newName);
        if (!result)
            return BadRequest("Record not found");
        return Ok(result);
    }

    [HttpDelete("{activityId}")]
    [Authorize]
    public async Task<ActionResult<bool>> DeleteActivityAsync(int activityId)
    {
        var result = await activityService.DeleteActivity(activityId);
        if (!result)
            return BadRequest("Record not found");
        return Ok(result);
    }

    [HttpGet("status/{activityId}")]
    [Authorize]
    public async Task<IActionResult> GetStatusByIdAsync(int activityId)
    {
        var status = await activityService.GetStatusById(activityId);
        if (status == 0)
            return NotFound("Records not found");
        return Ok(status);
    }

    [HttpPut("archive/{activityId}")]
    [Authorize]
    public async Task<ActionResult<bool>> PutActivityInArchive(int activityId)
    {
        var result = await activityService.PutActivityInArchive(activityId);
        if (!result)
            return BadRequest("Record not found");
        return Ok(result);
    }

    [HttpPut("recover/{activityId}")]
    [Authorize]
    public async Task<ActionResult<bool>> RecoverActivity(int activityId)
    {
        var result = await activityService.RecoverActivity(activityId);
        if (!result)
            return BadRequest("Record not found");
        return Ok(result);
    }

    [HttpPut("updateStatus/{activityId}/{newStatusId}")]
    [Authorize]
    public async Task<ActionResult<bool>> ChangeStatus(int activityId, int newStatusId)
    {
        var result = await activityService.ChangeStatus(activityId, newStatusId);
        if (!result)
            return BadRequest("Record not found");
        return Ok(result);
    }
}
