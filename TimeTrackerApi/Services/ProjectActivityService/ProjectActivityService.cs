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
    public async Task<ProjectActivity> AddProjectActivity(int activityId, string projectId)
    {
        Task<bool> flag = CheckProjectActivity(activityId, projectId);
        bool isExistInProjectActivities = await flag;

        bool isExistInProjects = await context.Projects.AnyAsync(p => p.Id == projectId);

        if (!isExistInProjectActivities & isExistInProjects)
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
    /// Проверка записи на существование в таблице "Активности проекта"
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<bool> CheckProjectActivity(int activityId, string projectId)
    {
        return await context.ProjectActivities.AnyAsync(a => a.ActivityId == activityId && a.ProjectId == projectId);
    }

    /// <summary>
    /// Удалить запись из таблицы "Активности проекта"
    /// </summary>
    /// <param name="activityId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteProjectActivity(int activityId, string projectId)
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
    public async Task<List<ProjectActivity>> GetActivitiesByProjectId(string projectId)
    {
        var query = context.ProjectActivities.AsQueryable();
        query = query.Where(a => a.ProjectId.Equals(projectId));
        return await query.ToListAsync(); 
    }

    /// <summary>
    /// Получить список проектов, в которых состоит активность
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
