﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTrackerApi.Services.ActivityPeriodService;
using TimeTrackerApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TimeTrackerApi.Services.ActivityService;
using System;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivityPeriodsController : ControllerBase
{
    private readonly IActivityPeriodService activityPeriodService;
    private readonly IActivityService activityService;

    public ActivityPeriodsController(IActivityPeriodService _userService,  IActivityService _activityService)
    {
        activityPeriodService = _userService;
        activityService = _activityService;
    }

    /// <summary>
    /// Получить статистику активности
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="data1"></param>
    /// <param name="data2"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<TimeSpan>> GetStatisticAsync(int activityId, DateTime? data1 = null, DateTime? data2 = null)
    {
        var activityExists = await activityService.GetActivityById(activityId);
        if (activityExists == null)
        {
            return NotFound($"Activity with ID {activityId} not found.");
        }

        TimeSpan? statistic = null;

        if (data1.HasValue && data2.HasValue) // промежуток времени
            statistic = await activityPeriodService.GetStatistic(activityId, data1, data2);

        // определенный день
        else if (data1.HasValue && !data2.HasValue)
            statistic = await activityPeriodService.GetStatistic(activityId, data1);
        else if (!data1.HasValue && data2.HasValue)
            statistic = await activityPeriodService.GetStatistic(activityId, data2);

        else
            statistic = await activityPeriodService.GetStatistic(activityId); //весь период

        if (statistic is null)
            return NotFound("No statistics found for the given period");
        return Ok(statistic);
    }

    /// <summary>
    /// Добавить данные об отслеживании активности
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="isStarted"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ActivityPeriod>> StartStopTracking([FromBody] StartStopTrackingDto dto)
    {
        var activityExists = await activityService.GetActivityById(dto.ActivityId);
        if (activityExists == null)
        {
            return NotFound($"Activity with ID {dto.ActivityId} not found.");
        }
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");

        var actPeriod = dto.IsStarted
           ? await activityPeriodService.StartTracking(dto.ActivityId)
           : await activityPeriodService.StopTracking(dto.ActivityId);

        if (actPeriod is null)
        {
            return BadRequest(dto.IsStarted ? "Failed to start tracking." : "Failed to stop tracking.");
        }

        var response = new ActivityPeriodDto
        {
            //actPeriod.Id,
            //actPeriod.ActivityId,
            //StartTime = TimeZoneInfo.ConvertTimeFromUtc(actPeriod.StartTime, tz),
            //StopTime = actPeriod.StopTime.HasValue
            //    ? TimeZoneInfo.ConvertTimeFromUtc(actPeriod.StopTime.Value, tz)
            //    : (DateTime?)null

            ActivityPeriodId = actPeriod.Id,
            ActId = actPeriod.ActivityId,
            Starttime = TimeZoneInfo.ConvertTimeFromUtc(actPeriod.StartTime, tz),
            Stoptime = TimeZoneInfo.ConvertTimeFromUtc(actPeriod.StopTime.Value, tz),
            Totaltime = actPeriod.TotalTime,
            Totalseconds = actPeriod.TotalSeconds,
        };
        return Ok(response);
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
    public async Task<ActionResult<bool>> UpdateTimeAsynс([FromBody] UpdatePeriod dto)
    {
        if (!dto.NewStartTime.HasValue && !dto.NewStopTime.HasValue)
            return BadRequest("At least one of newStartTime or newStopTime must has value.");

        var activityPeriod = await activityPeriodService.GetActivityPeriodById(dto.ActivityPeriodId);
        if (activityPeriod is null)
            return NotFound($"ActivityPeriod with ID {dto.ActivityPeriodId} not found.");

        ActivityPeriod? result = null;

        if (dto.NewStartTime.HasValue)
            result = await activityPeriodService.UpdateActivityPeriod(dto.ActivityPeriodId, dto.NewStartTime);

        if (dto.NewStopTime.HasValue)
            result = await activityPeriodService.UpdateActivityPeriod(dto.ActivityPeriodId, null, dto.NewStopTime);

        if (result is null)
            return BadRequest("Failed to update activity period.");

        return Ok(new
        {
            //result.Id,
            //result.ActivityId,
            //result.StartTime,
            //result.StopTime,
            //result.TotalTime,
            //result.TotalSeconds

            ActivityPeriodId = result.Id,
            ActId = result.ActivityId,
            Starttime = result.StartTime,
            Stoptime = result.StopTime,
            Totaltime = result.TotalTime,
            Totalseconds = result.TotalSeconds
        });
    }

    [HttpDelete("{activityPeriodId}")]
    [Authorize]
    public async Task<ActionResult> DeleteActivityPeriodAsync(int activityPeriodId)
    {
        var activityPeriod = await activityPeriodService.GetActivityPeriodById(activityPeriodId);
        if (activityPeriod is null)
            return NotFound($"ActivityPeriod with ID {activityPeriodId} not found.");

        var result = await activityPeriodService.DeleteActivityPeriod(activityPeriodId);
        if (!result)
            return StatusCode(500, "Failed to delete the activity period.");
        return NoContent();
    }
}

public class StartStopTrackingDto
{
    public int ActivityId { get; set; }
    public bool IsStarted { get; set; }
}

public class UpdatePeriod
{
    public int ActivityPeriodId { get; set; }
    public DateTime? NewStartTime { get; set; } = null;
    public DateTime? NewStopTime { get; set; } = null;
}

public class ActivityPeriodDto
{
    public int ActivityPeriodId { get; set; }
    public int ActId { get; set; }
    public DateTime? Starttime { get; set; } = null;
    public DateTime? Stoptime { get; set; } = null;
    public TimeSpan? Totaltime { get; set; } = null;
    public long? Totalseconds { get; set; } = 0;
}