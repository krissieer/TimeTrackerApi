using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TimeTrackerApi.Models;
using TimeTrackerApi.Services.ActivityService;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService activityService;

    public ActivitiesController(IActivityService _activityService)
    {
        activityService = _activityService;
    }

    //[HttpGet]
    //[Authorize]
    //public async Task<IActionResult> GetActivities(bool onlyArchived, bool onlyActive, int userId)
    //{
    //    List<Activity> activities = null;
    //    //все активности
    //    if (!onlyArchived && !onlyActive)
    //    {
    //        activities = await activityService.GetActivities(userId, false, false);
    //    }
    //    //только активные
    //    else if (onlyActive)
    //    {
    //        activities = await activityService.GetActivities(userId);
    //    }
    //    //только архивированные
    //    else if (onlyArchived)
    //    {
    //        activities = await activityService.GetActivities(userId, false, true);
    //    }
    //    if (activities.Count == 0)
    //        return NotFound("Records not found");
    //    return Ok(activities);
    //}



    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetStatusAsync(int activityId)
    {
        var status = await activityService.GetStatusById(activityId);
        if (status == 0)
            return NotFound("Records not found");
        return Ok(status);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<bool>> AddActivitiesAsync(int userId, string activityName, bool addDefault)
    {
        bool result;
        if (addDefault)
             result = await activityService.AddDefaultActivities(userId);
        else
            result = await activityService.AddActivity(userId, activityName);

        if (!result)
            return BadRequest("Activity with the same activityName already exists");

        return Ok(result);
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult<bool>> UpdateActivityAsynс(int activityId, bool updateName, bool archived, string newName = null)
    {
        bool result = false;

        if (updateName && newName is not null)
            result = await activityService.UpdateActivityName(activityId, newName);

        else if (archived)
            result = await activityService.ChangeStatus(activityId, 3);

        else if (!archived)
            result = await activityService.ChangeStatus(activityId, 1);

        if (!result)
            return NotFound("Record not found");

        return Ok(result);
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult<bool>> DeleteActivityAsync(int activityId)
    {
        var result = await activityService.DeleteActivity(activityId);
        if (!result)
            return BadRequest("Record not found");
        return Ok(result);
    }
}
