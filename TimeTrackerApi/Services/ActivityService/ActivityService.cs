using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ActivityService;

public class ActivityService: IActivityService
{
    private readonly TimeTrackerDbContext context;

    public ActivityService(TimeTrackerDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Получить список активностей пользователя
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="activeOnly"></param>
    /// <returns></returns>
    public async Task<List<Activity>> GetActivities(int userId, bool activeOnly = true, bool inProcessOnly = false, bool archivedOnly = false)
    {
        var query = context.Activities.AsQueryable();

        query = query.Where(a => a.UserId == userId);

        if (activeOnly && !inProcessOnly && !archivedOnly)
            query = query.Where(a => a.StatusId == 1);

        else if (!activeOnly && inProcessOnly && !archivedOnly)
            query = query.Where(a => a.StatusId == 2);

        else if (!activeOnly && !inProcessOnly && archivedOnly)
            query = query.Where(a => a.StatusId == 3);

        else if (activeOnly && inProcessOnly && !archivedOnly)
            query = query.Where(a => a.StatusId != 3);

        //if (activeOnly && !archivedOnly)
        //    query = query.Where(a => a.StatusId != 3);
        //else if (!activeOnly && archivedOnly)
        //    query = query.Where(a => a.StatusId == 3);

        return await query.ToListAsync();
    }

    public async Task<Activity?> GetActivityById(int activityId)
    {
        return await context.Activities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == activityId);
    }

    /// <summary>
    /// Добавить новую активность
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<bool> AddActivity(int userId, string name)
    {
        if (await CheckActivityNameExistence(userId, name))
            return false;
        else
        {
            var activity = new Activity
            {
                Name = name,
                UserId = userId,
                ActiveFrom = DateTime.Now.Date,
                StatusId = 1
            };
            await context.Activities.AddAsync(activity);
            return await context.SaveChangesAsync() >= 1;
        }
    }

    /// <summary>
    /// Проверить имя активности на сущестование у пользователя
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<bool> CheckActivityNameExistence(int userId, string name)
    {
        return await context.Activities.AnyAsync(a => a.UserId == userId && a.Name == name);
    }

    /// <summary>
    /// Обновить имя активности
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="newname"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<bool> UpdateActivityName(int activityId, string newname)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(newname))
            {
                throw new Exception("New name is empty");
            }    

            var activity = await context.Activities.FirstOrDefaultAsync(a => a.Id == activityId);
            if (activity == null)
            {
                throw new Exception("Activity not found.");
            }

            var user = activity.UserId;
            if (await CheckActivityNameExistence(user, newname))
            {
                throw new Exception("Username is already exist");
            }

            activity.Name = newname;
            return await context.SaveChangesAsync() >= 1;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error while updating activity name: {ex.Message}");
        }
    }

    /// <summary>
    /// Удалить активность
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteActivity(int activityId)
    {
        var activity = await context.Activities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == activityId);
        if (activity is null)
            throw new KeyNotFoundException($"Activity with ID {activityId} not found.");
        context.Activities.Remove(activity);
        return await context.SaveChangesAsync() >= 1;
    }

    /// <summary>
    /// Получить стутус активности
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public async Task<int> GetStatusById(int activityId)
    {
        var activity = await context.Activities.FirstOrDefaultAsync(u => u.Id == activityId);
        if (activity is null)
            return 0;
        return activity.StatusId;
    }

    /// <summary>
    /// Изменить статус активности
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="newStatusId"></param>
    /// <returns></returns>
    public async Task<bool> ChangeStatus(int activityId, int newStatusId)
    {
        var activity = await context.Activities.FindAsync(activityId);
        if (activity is null)
            return false;

        activity.StatusId = newStatusId;

        return await context.SaveChangesAsync() >= 1;
    }
}

