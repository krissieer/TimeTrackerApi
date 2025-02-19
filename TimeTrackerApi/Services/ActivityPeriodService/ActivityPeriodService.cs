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

    /// <summary>
    /// Добавить запись об отслеживании в БД
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public async Task<ActivityPeriod> AddActivityPeriod(int activityId)
    {
        var activityPeriod = new ActivityPeriod
        {
            ActivityId = activityId,
            StartTime = DateTime.Now,
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
    public async Task<ActivityPeriod> UpdateActivityPeriod(int activityPeriodId, DateTime? data1 = null, DateTime? data2 = null)
    {
        var activityPeriod = await context.ActivityPeriods.FirstOrDefaultAsync(a => a.Id == activityPeriodId);
        if (activityPeriod is null)
            return null;
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Yekaterinburg");

        if (data1.HasValue)
        {
            var local = DateTime.SpecifyKind(data1.Value, DateTimeKind.Unspecified);
            Console.WriteLine("entered localtime: " + local);
            var utcStartTime = TimeZoneInfo.ConvertTimeToUtc(local, timeZone);
            Console.WriteLine("converted UTC time: " + utcStartTime);
            activityPeriod.StartTime = DateTime.SpecifyKind(utcStartTime, DateTimeKind.Unspecified); 
        }
             
        if (data2.HasValue)
        {
            var local = DateTime.SpecifyKind(data2.Value, DateTimeKind.Unspecified);
            Console.WriteLine("entered localtime: " + local);
            var utcStopTime = TimeZoneInfo.ConvertTimeToUtc(local, timeZone);
            Console.WriteLine("converted UTC time: " + utcStopTime);
            activityPeriod.StopTime = DateTime.SpecifyKind(utcStopTime, DateTimeKind.Unspecified); 
        }

        DateTime startTime = activityPeriod.StartTime;
        DateTime stopTime = activityPeriod.StopTime;

        TimeSpan result = stopTime - startTime;
        activityPeriod.TotalTime = result;
        activityPeriod.TotalSeconds = (long)result.TotalSeconds;

        await context.SaveChangesAsync();

        return activityPeriod;
    }

    /// <summary>
    /// Начать отслеживание
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    //public async Task<int> StartTracking(int activityId)
    //{
    //    var result = await AddActivityPeriod(activityId);
    //    if (result is null)
    //        return 0;
    //    var activity = await context.Activities.FindAsync(activityId);
    //    if (activity is null)
    //    {
    //        Console.WriteLine("Активность пустая");
    //        return 0;
    //    }
    //    if (activity.StatusId == 2)
    //    {
    //        Console.WriteLine("Активность уже запущена");
    //        return -1;
    //    }
    //    activity.StatusId = 2;
    //    await context.SaveChangesAsync();
    //    return 1;
    //}

    public async Task<ActivityPeriod> StartTracking(int activityId)
    {
        var result = await AddActivityPeriod(activityId);
        if (result is null)
            return null;
        var activity = await context.Activities.FindAsync(activityId);

        if (activity is not null && activity.StatusId == 2)
        {
            Console.WriteLine("Активность уже запущена");
            return null;
        }
        activity.StatusId = 2;
        await context.SaveChangesAsync();
        return result;
    }

    /// <summary>
    /// Остановить отслеживание
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public async Task<ActivityPeriod> StopTracking(int activityId)
    {
        var result = await SetStopTime(activityId);
        //Console.WriteLine($"ЛОГ В StopTracking. Начало: {result.StartTime}, Конец: {result.StopTime}, Всего: {result.TotalTime}");
        if (result is null)
            return null;

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
    public async Task<ActivityPeriod> SetStopTime(int activityId)
    {
        var startTime = await GetStartTimeById(activityId);

        var activityPeriod = await context.ActivityPeriods.FirstOrDefaultAsync(a => a.ActivityId == activityId && a.StartTime == startTime);
        if (activityPeriod is null)
            return null;

        DateTime stopTime = DateTime.Now;
        DateTime time = DateTime.SpecifyKind(stopTime, DateTimeKind.Unspecified);
        activityPeriod.StopTime = time;
        TimeSpan result = stopTime - startTime;
        activityPeriod.TotalTime = result;
        activityPeriod.TotalSeconds = (long)result.TotalSeconds;

        await context.SaveChangesAsync();
        return activityPeriod;
    }

    /// <summary>
    /// Получить время начала отслеживания активности
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public async Task<DateTime> GetStartTimeById(int activityId)
    {
        var activityPeriod = await context.ActivityPeriods.OrderBy(a => a.Id).LastOrDefaultAsync(a => a.ActivityId == activityId);

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
    public async Task<TimeSpan> GetStatistic(int activityId, DateTime? firstdata = null, DateTime? seconddata = null)
    {
        var query = context.ActivityPeriods.AsQueryable();

        // статистика за промежуток времени
        if (firstdata.HasValue && seconddata.HasValue)
        {
            var startDate = firstdata.Value.Date;
            var endDate = seconddata.Value.Date;
            query = query.Where(a => a.ActivityId == activityId && a.StopTime >= startDate && a.StopTime < endDate.AddDays(1));
        }

        // статистика за определенный день
        else if (firstdata.HasValue)
        {
            var targetDate = firstdata.Value.Date;
            query = query.Where(a => a.ActivityId == activityId && a.StopTime >= targetDate && a.StopTime < targetDate.AddDays(1));
        }

        // за весь период
        else
            query = query.Where(a => a.ActivityId == activityId);

        var totalSeconds = await query.SumAsync(a => (long?)a.TotalSeconds) ?? 0;
        return TimeSpan.FromSeconds(totalSeconds);
    }
    // Запустить GetStatistic в цикле (по каждой необходимой активности - по дефолту все активности пользователя (получить список активностей))
    // Для того чтобы получить статистику за определенные проекты - получить активности,
    //  которые учпствуют в этих проектах + эти активности принадлежат текущему пользователю, и запустить GetStatistic для этого списка активностей

}
