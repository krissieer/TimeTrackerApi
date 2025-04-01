using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ProjectUserService;

public class ProjectUserService: IProjectUserService
{
    private readonly TimeTrackerDbContext context;

    public ProjectUserService(TimeTrackerDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Добавление пользователя в таблицу "Пользователи проекта"
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<ProjectUser> AddProjectUser(int userId, int projectId, bool isCreator)
    {
        //проверка, что такой проект существует и что у пользователя нет такого проекта
        bool isExist= await context.ProjectUsers.AnyAsync(p => p.ProjectId == projectId && p.UserId == userId); 
        if (!isExist)
        {
            var projectUser = new ProjectUser
            {
                UserId = userId,
                ProjectId = projectId,
                Creator = isCreator,
            };
            await context.ProjectUsers.AddAsync(projectUser);
            await context.SaveChangesAsync();
            return projectUser;
        }
        else return null;
    }

    /// <summary>
    /// Подключиться к проекту по коючу доступа
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="accessKey"></param>
    /// <returns></returns>
    public async Task<ProjectUser> ConnectToProject(int userId, string accessKey)
    {
        //проверка, что проект с таким ключом доступа есть
        var project = await context.Projects.FirstOrDefaultAsync(p => p.AccessKey == accessKey);
        if (project is not null)
        {
            var projectUser = new ProjectUser
            {
                UserId = userId,
                ProjectId = project.Id,
                Creator = false
            };
            await context.ProjectUsers.AddAsync(projectUser);
            await context.SaveChangesAsync();
            return projectUser;
        }
        else return null;
    }

    /// <summary>
    /// Удаление записи из таблицы "Пользователи проекта" 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteProjectUser(int userId, int projectId)
    {
        var projectUser = await context.ProjectUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId && a.ProjectId == projectId);
        if (projectUser == null)
            throw new KeyNotFoundException($"User with ID {userId} not found in project with ID {projectId}.");
        context.ProjectUsers.Remove(projectUser);
        return await context.SaveChangesAsync() >= 1;
    }

    /// <summary>
    /// Получить список проектов пользователя
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<List<ProjectUser>> GetProjectsByUserId(int userId)
    {
        var query = context.ProjectUsers.AsQueryable();
        query = query.Where(a => a.UserId == userId);
        return await query.ToListAsync();
    }

    /// <summary>
    /// Получить список пользователей проекта 
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<List<ProjectUser>> GetUsersByProjectId(int projectId)
    {
        var query = context.ProjectUsers.AsQueryable();
        query = query.Where(a => a.ProjectId == projectId);
        return await query.ToListAsync();
    }

    /// <summary>
    /// Проверить, является ли пользователь создателем проекта
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<bool> IsCreator(int userId, int projectId)
    {
        return await context.ProjectUsers
            .Where(p => p.ProjectId == projectId && p.UserId == userId)
            .Select(a => a.Creator)
            .SingleOrDefaultAsync();
    }
}
