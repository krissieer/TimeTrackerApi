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

    public async Task<bool> CheckProjectIdExistence(string id)
    {
        return await context.Projects.AnyAsync(p => p.Id == id);
    }

    public async Task<Project> UpdateProject(string id, string newName)
    {
        var project = await context.Projects.FirstOrDefaultAsync(a => a.Id == id);
        if (project == null)
            return null;
        project.Name = newName;
        await context.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteProject(string id)
    {
        var project = await context.Projects.FirstOrDefaultAsync(a => a.Id == id);
        if (project == null) 
            return false;
        context.Projects.Remove(project);
        return await context.SaveChangesAsync() >= 1;
    }

    public async Task<string> GetProjectNameById(string id)
    {
        var project = await context.Projects.FirstOrDefaultAsync(a => a.Id == id);
        if (project == null)
            return null;
        return project.Name;
    }
}
