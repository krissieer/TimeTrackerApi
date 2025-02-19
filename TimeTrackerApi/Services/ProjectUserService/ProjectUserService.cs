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
    public async Task<ProjectUser> AddProjectUser(int userId, string projectId, bool isCreator)
    {
        Task<bool> flag1 = CheckProjectIdExistence(projectId); // проверка, что такой проект существует
        bool isExistInProjects = await flag1;

        Task<bool> flag2 = CheckProjectUser(userId, projectId);  // проверка, что у пользователя нет такого проекта
        bool isExistInProjectUsers = await flag2;

        if (isExistInProjects && !isExistInProjectUsers)
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
    /// Проверка сущестования проекта по Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> CheckProjectIdExistence(string id)
    {
        return await context.Projects.AnyAsync(p => p.Id == id);
    }

    /// <summary>
    /// Проверка записи на существование в таблице "Пользователи проекта" 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<bool> CheckProjectUser(int userId, string projectId)
    {
        return await context.ProjectUsers.AnyAsync(p => p.ProjectId == projectId && p.UserId == userId);
    }

    /// <summary>
    /// Удаление записи из таблицы "Пользователи проекта" 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="projectId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteProjectUser(int userId, string projectId)
    {
        var projectUser = await context.ProjectUsers.FirstOrDefaultAsync(a => a.UserId == userId && a.ProjectId == projectId);
        if (projectUser == null)
            return false;
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
    public async Task<List<ProjectUser>> GetUsersByProjectId(string projectId)
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
    public async Task<bool> IsCreator(int userId, string projectId)
    {
        return await context.ProjectUsers
            .Where(p => p.ProjectId == projectId && p.UserId == userId)
            .Select(a => a.Creator)
            .SingleOrDefaultAsync();
    }
}
