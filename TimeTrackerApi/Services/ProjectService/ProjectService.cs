using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text;
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
    /// Получить список проектов
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

    public async Task<Project> GetProjectById(int Id)
    {
        var project = await context.Projects.FirstOrDefaultAsync(p => p.Id == Id);
        if (project is not null)
            return project;
        else return null;
    }

    /// <summary>
    /// Добавить проект - Создание проекта
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<Project> AddProject(string name)
    {
        bool flag = true;
        string accessKey = string.Empty;
        while (flag)
        {
            accessKey = GenerateKey();
            flag = await context.Projects.AnyAsync(p => p.AccessKey == accessKey);
        }
        if (!string.IsNullOrEmpty(accessKey))
        {
            var project = new Project
            {
                Name = name,
                AccessKey = accessKey,
            };
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();
            return project;
        }
        return null;
    }

    /// <summary>
    /// Генерация ключа доступа
    /// </summary>
    /// <returns></returns>
    public string GenerateKey()
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var key = new StringBuilder();
        Random rnd = new Random();
        for (int i = 0; i < 8; i++)
        {
            key.Append(chars[rnd.Next(chars.Length)]);
        }
        return key.ToString();
    }

    /// <summary>
    /// Обновить название проекта
    /// </summary>
    /// <param name="id"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    public async Task<Project> UpdateProject(int id, string newName)
    {
        var project = await context.Projects.FirstOrDefaultAsync(a => a.Id == id);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {id} not found.");
        project.Name = newName;
        await context.SaveChangesAsync();
        return project;
    }

    /// <summary>
    /// Удалить проект
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteProject(int id)
    {
        var project = await context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
        if (project == null)
            throw new KeyNotFoundException($"Project with ID {id} not found.");
        context.Projects.Remove(project);
        return await context.SaveChangesAsync() >= 1;
    }
}
