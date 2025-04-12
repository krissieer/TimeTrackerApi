using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTrackerApi.Services.ActivityPeriodService;
using TimeTrackerApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TimeTrackerApi.Services.ActivityService;
using System;
using System.Text.Json.Serialization;
using System.Security.Claims;
using TimeTrackerApi.Services.UserService;

namespace TimeTrackerApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivityPeriodsController : ControllerBase
{
    private readonly IActivityPeriodService activityPeriodService;
    private readonly IActivityService activityService;
    private readonly IUserService userService;

    public ActivityPeriodsController(IActivityPeriodService _activityPeriodService,  IActivityService _activityService, IUserService _userService)
    {
        activityPeriodService = _activityPeriodService;
        activityService = _activityService;
        userService = _userService;
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
    public async Task<ActionResult> GetStatistic(int activityId = 0, int userId = 0, DateTime? data1 = null, DateTime? data2 = null)
    {
        if (activityId != 0)
        {
            var activityExists = await activityService.GetActivityById(activityId);
            if (activityExists == null)
                return NotFound($"Activity with ID {activityId} not found.");
        }

        if (userId != 0)
        {
            var userExist = await userService.GetUserById(userId);
            if (userExist == null)
                return NotFound($"User with ID {userId} not found.");
        }
        
        var statistic = new List<ActivityPeriod>();
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");

        if (data1.HasValue && data2.HasValue) // промежуток времени
            statistic = await activityPeriodService.GetStatistic(activityId, userId, data1, data2);

        // определенный день
        else if (data1.HasValue && !data2.HasValue)
            statistic = await activityPeriodService.GetStatistic(activityId, userId, data1);
        else if (!data1.HasValue && data2.HasValue)
            statistic = await activityPeriodService.GetStatistic(activityId, userId, data2);
        else
            statistic = await activityPeriodService.GetStatistic(activityId, userId); //весь период

        if (!statistic.Any())
            return Ok(new List<ActivityPeriod>());

        var result = statistic.Select(a => new ActivityPeriodDto
        {
            activityPeriodId = a.Id,
            activityId = a.ActivityId,
            executorId = a.ExecutorId,
            startTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(a.StartTime, DateTimeKind.Utc), tz),
            stopTime = a.StopTime.HasValue
               ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(a.StopTime.Value, DateTimeKind.Utc), tz)
               : (DateTime?)null,
            totalTime =  a.TotalTime,
        });

        return Ok(result);
    }

    /// <summary>
    /// Добавить данные об отслеживании активности
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="isStarted"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> StartStopTracking([FromBody] StartStopTrackingDto dto)
    {
        var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(user))
            return Unauthorized("User not authenticated.");
        int authorizedUserId = int.Parse(user);

        var activityExists = await activityService.GetActivityById(dto.activityId);
        if (activityExists == null)
        {
            return NotFound($"Activity with ID {dto.activityId} not found.");
        }
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");

        List<ActivityPeriod> actPeriod = dto.isStarted
            ? new List<ActivityPeriod> { await activityPeriodService.StartTracking(dto.activityId, authorizedUserId) }
            : await activityPeriodService.StopTracking(dto.activityId, authorizedUserId);

        if (actPeriod is null || !actPeriod.Any())
        {
            return BadRequest(dto.isStarted ? "Failed to start tracking." : "Failed to stop tracking.");
        }

        var response = actPeriod.Select(actPeriod => new ActivityPeriodDto
        {
            activityPeriodId = actPeriod.Id,
            activityId = actPeriod.ActivityId,
            executorId = actPeriod.ExecutorId,
            startTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(actPeriod.StartTime, DateTimeKind.Utc), tz),
            stopTime = actPeriod.StopTime.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(actPeriod.StopTime.Value, DateTimeKind.Utc), tz)
                    : (DateTime?)null,
            totalTime = actPeriod.TotalTime,
        }).ToList();

        return Ok(response);
    }

    /// <summary>
    /// Обновить время в записи отслеживания
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="activityPeriodId"></param>
    /// <returns></returns>
    [HttpPut("{activityPeriodId}")]
    [Authorize]
    public async Task<ActionResult> UpdateTime([FromBody] UpdatePeriodDto dto, int activityPeriodId)
    {
        var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(user))
            return Unauthorized("User not authenticated.");
        int authorizedUserId = int.Parse(user);

        if (!dto.newStartTime.HasValue && !dto.newStopTime.HasValue)
            return BadRequest("At least one of newStartTime or newStopTime must has value.");

        var activityPeriod = await activityPeriodService.GetActivityPeriodById(activityPeriodId);
        if (activityPeriod is null)
            return NotFound($"ActivityPeriod with ID {activityPeriodId} not found.");

        List<ActivityPeriod>? result = null;

        if (dto.newStartTime.HasValue && dto.newStopTime.HasValue)
            result = await activityPeriodService.UpdateActivityPeriod(activityPeriodId, authorizedUserId, dto.newStartTime, dto.newStopTime);

        else if (dto.newStartTime.HasValue && !dto.newStopTime.HasValue)
            result = await activityPeriodService.UpdateActivityPeriod(activityPeriodId, authorizedUserId, dto.newStartTime);

        else if (dto.newStopTime.HasValue && !dto.newStartTime.HasValue)
            result = await activityPeriodService.UpdateActivityPeriod(activityPeriodId, authorizedUserId, null, dto.newStopTime);

        if (result is null)
            return BadRequest("You can not edit this record");

        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");
        var resultDto = result.Select(period => new ActivityPeriodDto
        {
            activityPeriodId = period.Id,
            activityId = period.ActivityId,
            executorId = period.ExecutorId,
            startTime = TimeZoneInfo.ConvertTimeFromUtc(period.StartTime, tz),
            stopTime = period.StopTime.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(period.StopTime.Value, tz) : null,
            totalTime = period.TotalTime,
        }).ToList();

        return Ok(resultDto);
    }

    /// <summary>
    /// Удалить запись отслеживания
    /// </summary>
    /// <param name="activityPeriodId"></param>
    /// <returns></returns>
    [HttpDelete("{activityPeriodId}")]
    [Authorize]
    public async Task<IActionResult> DeleteActivityPeriod(int activityPeriodId)
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
    public int activityId { get; set; }
    public bool isStarted { get; set; }
}

public class UpdatePeriodDto
{
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime? newStartTime { get; set; } = null;
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime? newStopTime { get; set; } = null;
}

public class ActivityPeriodDto
{
    public int activityPeriodId { get; set; }
    public int activityId { get; set; }
    public int executorId { get; set; }
    public DateTime? startTime { get; set; } = null;
    public DateTime? stopTime { get; set; } = null;
    public TimeSpan? totalTime { get; set; } = null;
}