using Microsoft.EntityFrameworkCore;
using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ProjectService;

public class ProjectService: IProjectService
{
    private readonly TimeTrackerDbContext context;

    public ProjectService(TimeTrackerDbContext context)
    {
        this.context = context;
    }

    /// <summary>
    /// Добавить проект - Создание проекта
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<Project> AddProject(string id, string name)
    {
        Task<bool> flag = CheckProjectIdExistence(id);
        bool isExist = await flag;
        if (!isExist)
        {
            var project = new Project
            {
                Id = id,
                Name = name,
            };
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();
            return project;
        }
        else return null;
    }

    /// <summary>
    /// Проверка на сущетсование проекта
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> CheckProjectIdExistence(string id)
    {
        return await context.Projects.AnyAsync(p => p.Id == id);
    }

    /// <summary>
    /// Обновить название проекта
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    public async Task<Project> UpdateProject(string id, string newName)
    {
        var project = await context.Projects.FirstOrDefaultAsync(a => a.Id == id);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {id} not found.");
        project.Name = newName;
        await context.SaveChangesAsync();
        Console.WriteLine("Cuerrent Name: " + project.Name);
        return project;
    }

    /// <summary>
    /// Удалить проект
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteProject(string id)
    {
        var project = await context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {id} not found.");
        context.Projects.Remove(project);
        return await context.SaveChangesAsync() >= 1;
    }

    /// <summary>
    /// Получить название проекта по его ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<List<Project>> GetProjects()
    {
        var projects = await context.Projects.ToListAsync();
        if (projects == null)
            return null;
        return projects;
    }
}
