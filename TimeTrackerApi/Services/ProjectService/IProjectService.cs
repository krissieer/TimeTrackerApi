using TimeTrackerApi.Models;

namespace TimeTrackerApi.Services.ProjectService
{
    public interface IProjectService
    {
        Task<Project> AddProject(string id, string name);

        Task<bool> CheckProjectIdExistence(string id);

        Task<Project> UpdateProject(string id, string newName);

        Task<bool> DeleteProject(string id);

        Task<List<Project>> GetProjects();
    }
}
