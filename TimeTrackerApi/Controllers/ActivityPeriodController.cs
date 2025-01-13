using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTrackerApi.Services.ActivityPeriodService;
using TimeTrackerApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivityPeriodController : ControllerBase
{
    private readonly IActivityPeriodService activityPeriodService;

    public ActivityPeriodController(IActivityPeriodService _userService)
    {
        activityPeriodService = _userService;
    }

    [HttpPut("update/starttime/{activityPeriodId}")]
    [Authorize]
    public async Task<ActionResult<bool>> UpdateStartTimeAsynс(int activityPeriodId, DateTime newStartTime)
    {
        var result = await activityPeriodService.UpdateActivityPeriod(activityPeriodId, newStartTime);
        if (result is null)
            return BadRequest("Record not found");
        return Ok(result);
    }

    [HttpPut("update/stoptime/{activityPeriodId}")]
    [Authorize]
    public async Task<ActionResult<bool>> UpdateStopTimeAsynс(int activityPeriodId, DateTime newStopTime)
    {
        var result = await activityPeriodService.UpdateActivityPeriod(activityPeriodId, null, newStopTime);
        if (result is null)
            return BadRequest("Record not found");
        return Ok(result);
    }


    [HttpPut("start/{activityId}")]
    [Authorize]
    public async Task<IActionResult> StartTracking(int activityId)
    {
        var result = await activityPeriodService.StartTracking(activityId);
        if (result == 0)
            return BadRequest("Failed to start tracking.");
        else if (result == -1)
            return BadRequest("Activity is already tracking");
        return Ok("Activity start tracking");
    }

    [HttpPut("stop/{activityId}")]
    [Authorize]
    public async Task<ActionResult<string>> StopTracking(int activityId)
    {
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");
        var result = await activityPeriodService.StopTracking(activityId);
        if (result is null)
            return BadRequest("Failed to stop tracking.");

        string formattedResult = $"StartTime: {TimeZoneInfo.ConvertTimeFromUtc(result.StartTime, tz)}, " +
                                 $"StopTime: {TimeZoneInfo.ConvertTimeFromUtc(result.StopTime, tz)}, " +
                                 $"TotalTime: {result.TotalTime}";
        return Ok(formattedResult);
    }

    [HttpDelete("{activityPeriodId}")]
    [Authorize]
    public async Task<ActionResult<bool>> DeleteActivityAsync(int activityPeriodId)
    {
        var result = await activityPeriodService.DeleteActivityPeriod(activityPeriodId);
        if (!result)
            return BadRequest("Record not found");
        return Ok(result);
    }

    [HttpGet("allperiod/{activityId}")]
    [Authorize]
    public async Task<ActionResult<TimeSpan>> GetAllStatisticAsync(int activityId)
    {
        var statistic = await activityPeriodService.GetStatistic(activityId);
        if (statistic == TimeSpan.Zero)
            return BadRequest("Records not found");
        return Ok(statistic);
    }

    [HttpGet("oneday/{activityId}")]
    [Authorize]
    public async Task<ActionResult<TimeSpan>> GetOnedayStatisticAsync(int activityId, DateTime date)
    {
        var statistic = await activityPeriodService.GetStatistic(activityId, date);
        if (statistic == TimeSpan.Zero)
            return BadRequest("Records not found");
        return Ok(statistic);
    }

    [HttpGet("interval/{activityId}/{date1}/{date2}")]
    [Authorize]
    public async Task<ActionResult<TimeSpan>> GetIntervalStatisticAsync(int activityId, DateTime date1, DateTime date2)
    {
        var statistic = await activityPeriodService.GetStatistic(activityId, date1, date2);
        if (statistic == TimeSpan.Zero)
            return BadRequest("Records not found");
        return Ok(statistic);
    }
}
