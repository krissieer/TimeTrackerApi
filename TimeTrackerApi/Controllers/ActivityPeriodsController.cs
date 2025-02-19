using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTrackerApi.Services.ActivityPeriodService;
using TimeTrackerApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivityPeriodsController : ControllerBase
{
    private readonly IActivityPeriodService activityPeriodService;

    public ActivityPeriodsController(IActivityPeriodService _userService)
    {
        activityPeriodService = _userService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<TimeSpan>> GetStatisticAsync(int activityId, DateTime? data1 = null, DateTime? data2 = null)
    {
        TimeSpan statistic = TimeSpan.Zero;

        if (data1.HasValue & data2.HasValue) // промежуток времени
            statistic = await activityPeriodService.GetStatistic(activityId, data1, data2);

        // определенный день
        else if (data1.HasValue & !data2.HasValue)
            statistic = await activityPeriodService.GetStatistic(activityId, data1);
        else if (!data1.HasValue & data2.HasValue)
            statistic = await activityPeriodService.GetStatistic(activityId, data2);

        else
            statistic = await activityPeriodService.GetStatistic(activityId); //вест период

        if (statistic == TimeSpan.Zero)
            return BadRequest("Records not found");
        return Ok(statistic);
    }


    [HttpPost]
    [Authorize]
    public async Task<IActionResult> StartStopTracking(int activityId, bool start)
    {
        ActivityPeriod result;
        if (start)
            result = await activityPeriodService.StartTracking(activityId);
        else
            result = await activityPeriodService.StopTracking(activityId);

        if (result is null)
            return BadRequest("Failed to start/stop tracking.");

        return Ok(result);
    }
    //public async Task<IActionResult> StartTracking(int activityId)
    //{
    //    var result = await activityPeriodService.StartTracking(activityId);
    //    if (result == 0)
    //        return BadRequest("Failed to start tracking.");
    //    else if (result == -1)
    //        return BadRequest("Activity is already tracking");
    //    return Ok("Activity start tracking");
    //}
    //public async Task<ActionResult<string>> StopTracking(int activityId)
    //{
    //    TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");
    //    var result = await activityPeriodService.StopTracking(activityId);
    //    if (result is null)
    //        return BadRequest("Failed to stop tracking.");

    //    string formattedResult = $"StartTime: {TimeZoneInfo.ConvertTimeFromUtc(result.StartTime, tz)}, " +
    //                             $"StopTime: {TimeZoneInfo.ConvertTimeFromUtc(result.StopTime, tz)}, " +
    //                             $"TotalTime: {result.TotalTime}";
    //    return Ok(formattedResult);
    //}


    [HttpPut]
    [Authorize]
    public async Task<ActionResult<bool>> UpdateTimeAsynс(int activityPeriodId, DateTime? newStartTime = null, DateTime? newStopTime = null)
    {
        ActivityPeriod result = null;

        if (newStartTime.HasValue)
            result = await activityPeriodService.UpdateActivityPeriod(activityPeriodId, newStartTime);

        if (newStopTime.HasValue)
            result = await activityPeriodService.UpdateActivityPeriod(activityPeriodId, null, newStopTime);

        if (result is null)
            return NotFound("Record not found");

        return Ok(result);
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult<bool>> DeleteActivityAsync(int activityPeriodId)
    {
        var result = await activityPeriodService.DeleteActivityPeriod(activityPeriodId);
        if (!result)
            return BadRequest("Record not found");
        return Ok(result);
    }
}
