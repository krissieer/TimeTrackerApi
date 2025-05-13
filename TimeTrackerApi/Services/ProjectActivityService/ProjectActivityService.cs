using Microsoft.EntityFrameworkCore;
using TimeTrackerApi.Models;
using System.Xml.Linq;

namespace TimeTrackerApi.Services.ProjectActivityService;

public class ProjectActivityService: IProjectActivityService
{
    private readonly TimeTrackerDbContext context;

    public ProjectActivityService(TimeTrackerDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Добавить запись об активности проекта в таблицу "Активности проекта"
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<ProjectActivity> AddProjectActivity(int activityId, int projectId)
    {
        //проверить, что активность есть в проекте
        bool isExist = await context.ProjectActivities.AnyAsync(a => a.ActivityId == activityId && a.ProjectId == projectId);

        if (!isExist)
        {
            var projectActivity = new ProjectActivity
            {
                ProjectId = projectId,
                ActivityId = activityId,
            };
            await context.ProjectActivities.AddAsync(projectActivity);
            await context.SaveChangesAsync();
            return projectActivity;
        }
        return null;
    }

    /// <summary>
    /// Удалить запись из таблицы "Активности проекта"
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteProjectActivity(int activityId, int projectId)
    {
        var projectActivity = await context.ProjectActivities
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ActivityId == activityId && a.ProjectId == projectId);
        if (projectActivity == null)
            throw new KeyNotFoundException($"Activity with ID {activityId} not found in project with ID {projectId}.");
       
        context.ProjectActivities.Remove(projectActivity);
        return await context.SaveChangesAsync() >= 1;
    }

    /// <summary>
    /// Получить список активностей проекта
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<List<ProjectActivity>> GetActivitiesByProjectId(int projectId)
    {
        var query = context.ProjectActivities.AsQueryable();
        query = query.Where(a => a.ProjectId == projectId);
        return await query.ToListAsync(); 
    }

    /// <summary>
    /// Получить список проектов, в которых участвует активность
    /// </summary>
    /// <param name="activityId"></param>
    /// <returns></returns>
    public async Task<List<ProjectActivity>> GetProjectsByActivityId(int activityId)
    {
        var query = context.ProjectActivities.AsQueryable();
        query = query.Where(a => a.ActivityId == activityId);
        return await query.ToListAsync(); 
    }
}
