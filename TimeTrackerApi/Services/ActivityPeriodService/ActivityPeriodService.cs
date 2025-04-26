using Microsoft.EntityFrameworkCore;
using System;
using System.Xml;
using TimeTrackerApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TimeTrackerApi.Services.ActivityPeriodService;

public class ActivityPeriodService: IActivityPeriodService
{
    private readonly TimeTrackerDbContext context;

    public ActivityPeriodService(TimeTrackerDbContext context)
    {
        this.context = context;
    }

    public async Task<ActivityPeriod?> GetActivityPeriodById(int activityPeriodId)
    {
        return await context.ActivityPeriods
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == activityPeriodId);
    }

    /// <summary>
    /// Добавить запись об отслеживании в БД
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public async Task<ActivityPeriod> AddActivityPeriod(int activityId, int userId)
    {
        var activityPeriod = new ActivityPeriod
        {
            ActivityId = activityId,
            StartTime = DateTime.Now,
            ExecutorId = userId
        };
        await context.ActivityPeriods.AddAsync(activityPeriod);
        await context.SaveChangesAsync();
        return activityPeriod;
    }

    /// <summary>
    /// Обновить время начала/конца отслеживания
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="data1"></param>
    /// <param name="date2"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<ActivityPeriod>> UpdateActivityPeriod(int activityPeriodId, int userId, DateTime? data1 = null, DateTime? data2 = null)
    {
        var activityPeriod = await context.ActivityPeriods.FirstOrDefaultAsync(a => a.Id == activityPeriodId && a.ExecutorId == userId);
        if (activityPeriod is null)
            return null;
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");

        if (data1.HasValue)
        {
            var local = DateTime.SpecifyKind(data1.Value, DateTimeKind.Unspecified);
            var utcStartTime = TimeZoneInfo.ConvertTimeToUtc(local, timeZone);
            activityPeriod.StartTime = DateTime.SpecifyKind(utcStartTime, DateTimeKind.Unspecified); 
        }
             
        if (data2.HasValue)
        {
            var local = DateTime.SpecifyKind(data2.Value, DateTimeKind.Unspecified);
            var utcStopTime = TimeZoneInfo.ConvertTimeToUtc(local, timeZone);
            activityPeriod.StopTime = DateTime.SpecifyKind(utcStopTime, DateTimeKind.Unspecified); 
        }

        if (activityPeriod.StopTime.HasValue && activityPeriod.StopTime <= activityPeriod.StartTime)
        {
            throw new InvalidOperationException("StopTime must be greater than StartTime.");
        }

        DateTime startTime = activityPeriod.StartTime;
        DateTime? stopTime = activityPeriod.StopTime;

        var result = new List<ActivityPeriod>();
        if (startTime.Date == stopTime.Value.Date) //время остановки и время начала в пределах одного дня
        {
            TimeSpan? totalDuration = stopTime - startTime;
            activityPeriod.TotalTime = totalDuration;
            await context.SaveChangesAsync();
            result.Add(activityPeriod);
        }
        else // Если даты разные 
        {
            var endOfStartDay = DateTime.SpecifyKind(startTime.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Unspecified);
            var utcEndOfStartDay = TimeZoneInfo.ConvertTimeToUtc(endOfStartDay, timeZone);
            activityPeriod.StopTime = DateTime.SpecifyKind(utcEndOfStartDay, DateTimeKind.Unspecified);
            var firstDuration = utcEndOfStartDay - startTime;
            activityPeriod.TotalTime = firstDuration;
            result.Add(activityPeriod);

            var secondStart = DateTime.SpecifyKind(endOfStartDay.AddSeconds(1), DateTimeKind.Unspecified);
            var utcsecondStart = TimeZoneInfo.ConvertTimeToUtc(secondStart, timeZone);
            var secondDuration = stopTime - utcsecondStart;
            var newPeriod = new ActivityPeriod
            {
                ActivityId = activityPeriod.ActivityId,
                ExecutorId = activityPeriod.ExecutorId,
                StartTime = DateTime.SpecifyKind(utcsecondStart, DateTimeKind.Unspecified),
                StopTime = stopTime,
                TotalTime = secondDuration,
            };

            await context.ActivityPeriods.AddAsync(newPeriod);
            await context.SaveChangesAsync();
            result.Add(newPeriod);
        }

        return result;
    }

    public async Task<ActivityPeriod> StartTracking(int activityId, int userId)
    {
        var result = await AddActivityPeriod(activityId, userId);
        if (result is null)
            return null;
        var activity = await context.Activities.FindAsync(activityId);
        if (activity.StatusId == 2)
            throw new Exception("Activity was already started");
        activity.StatusId = 2;
        await context.SaveChangesAsync();
        return result;
    }

    /// <summary>
    /// Остановить отслеживание
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public async Task<List<ActivityPeriod>> StopTracking(int activityId, int userId)
    {
        var result = await SetStopTime(activityId, userId);
        if (result is null)
            throw new Exception("Failed to stop");
        var activity = await context.Activities.FindAsync(activityId);
        activity.StatusId = 1;
        await context.SaveChangesAsync();
        return result;
    }

    /// <summary>
    /// Установить время конца отслеживания
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<ActivityPeriod>> SetStopTime(int activityId, int userId)
    {
        var startTime = await GetStartTimeById(activityId, userId);

        var activityPeriod = await context.ActivityPeriods.FirstOrDefaultAsync(a => a.ActivityId == activityId && a.StartTime == startTime);

        if (activityPeriod is null)
            throw new Exception("ActivityPeriod not found.");

        DateTime stopTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        var result = new List<ActivityPeriod>();

        if (startTime.Date == stopTime.Date) //дата начала и конца в пределах одного дня
        {
            activityPeriod.StopTime = stopTime;
            var diff = stopTime - startTime;
            activityPeriod.TotalTime = diff;

            await context.SaveChangesAsync();
            result.Add(activityPeriod);
            return result;
        }
        else //если дата начала и конца отслеживания разные
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");

            var endOfStartDay = DateTime.SpecifyKind(startTime.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Unspecified);
            var utcEndOfStartDay = TimeZoneInfo.ConvertTimeToUtc(endOfStartDay, timeZone);
            activityPeriod.StopTime = DateTime.SpecifyKind(utcEndOfStartDay, DateTimeKind.Unspecified);
            var firstDuration = utcEndOfStartDay - startTime;
            activityPeriod.TotalTime = firstDuration;
            result.Add(activityPeriod);

            var secondStart = DateTime.SpecifyKind(endOfStartDay.AddSeconds(1), DateTimeKind.Unspecified);
            var utcsecondStart = TimeZoneInfo.ConvertTimeToUtc(secondStart, timeZone);
            var secondDuration = stopTime - utcsecondStart;
            var newPeriod = new ActivityPeriod
            {
                ActivityId = activityPeriod.ActivityId,
                ExecutorId = activityPeriod.ExecutorId,
                StartTime = DateTime.SpecifyKind(utcsecondStart, DateTimeKind.Unspecified),
                StopTime = stopTime,
                TotalTime = secondDuration,
            };

            await context.ActivityPeriods.AddAsync(newPeriod);
            await context.SaveChangesAsync();
            result.Add(newPeriod);
            return result;
        }
    }

    /// <summary>
    /// Получить время начала отслеживания активности
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public async Task<DateTime> GetStartTimeById(int activityId, int userId)
    {
        var activityPeriod = await context.ActivityPeriods.OrderBy(a => a.Id).LastOrDefaultAsync(a => a.ActivityId == activityId && a.ExecutorId == userId);

        if (activityPeriod == null)
            throw new Exception("Start time not found for the given activity ID.");

        return activityPeriod.StartTime;    
    }

    /// <summary>
    /// Удалить запись об отслежживании
    /// </summary>
    /// <param name="activityPeriodId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteActivityPeriod(int activityPeriodId)
    {
        var activityPeriod = await context.ActivityPeriods.FindAsync(activityPeriodId);
        if (activityPeriod is null)
            return false;

        context.ActivityPeriods.Remove(activityPeriod);
        return await context.SaveChangesAsync() >= 1;
    }

    /// <summary>
    /// Получение статистики
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="firstdata"></param>
    /// <param name="seconddata"></param>
    /// <returns></returns>
    public async Task<List<ActivityPeriod>> GetStatistic(int activityId = 0, int userId = 0, DateTime? firstdata = null, DateTime? seconddata = null)
    {
        var query = context.ActivityPeriods.AsQueryable();
        if (activityId != 0)
            query = query.Where(a => a.ActivityId == activityId);
        if (userId != 0)
            query = query.Where(a => a.ExecutorId == userId);

        // статистика за промежуток времени
        if (firstdata.HasValue && seconddata.HasValue)
        {
            var startDate = firstdata.Value.Date;
            var endDate = seconddata.Value.Date;
            query = query.Where(a => a.StopTime >= startDate && a.StopTime < endDate.AddDays(1));
        }

        // статистика за определенный день
        else if (firstdata.HasValue)
        {
            var targetDate = firstdata.Value.Date;
            query = query.Where(a => a.StopTime >= targetDate && a.StopTime < targetDate.AddDays(1));
        }

        return await query.ToListAsync();
    }
    // Запустить GetStatistic в цикле (по каждой необходимой активности - по дефолту все активности пользователя (получить список активностей))
    // Для того чтобы получить статистику за определенные проекты - получить активности,
    // которые учпствуют в этих проектах + эти активности принадлежат текущему пользователю, и запустить GetStatistic для этого списка активностей

}
